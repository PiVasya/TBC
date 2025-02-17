using System.Diagnostics;
using Telegram.Bot.Types;

public static class DockerBotBuilder
{
    public static async Task<string> CreateAndRunBotAsync(string telegramToken)
    {
        // 1. Создаём уникальную папку для файлов
        //    Например: /tmp/bot_XXXXXXXX
        string folderName = "bot_" + Guid.NewGuid().ToString("N");
        string basePath = Path.Combine("/tmp", folderName);
        Directory.CreateDirectory(basePath);

        // 2. Пишем .csproj
        //    Включаем ссылку на Telegram.Bot
        //    Авто-компиляция *.cs (EnableDefaultCompileItems=true по умолчанию)
        string csprojContent = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Telegram.Bot"" Version=""22.4.2"" />
  </ItemGroup>
</Project>";
        File.WriteAllText(Path.Combine(basePath, "BotCode.csproj"), csprojContent);

        // 3. Пишем Program.cs
        //    Вместо "Нажмите Enter..." делаем бесконечное ожидание,
        //    чтобы бот жил, пока контейнер не будет остановлен.
        string programCsContent = $@"using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyGeneratedBot
{{
    class Program
    {{
        static async Task Main()
        {{
            var botClient = new TelegramBotClient(""{telegramToken}"");

            // Используем метод без Async-суффикса
            await botClient.DeleteWebhook();

            botClient.StartReceiving(
                UpdateHandler,
                ErrorHandler
            );

            Console.WriteLine(""Бот запущен. Ожидаю сообщения..."");

            // Бесконечная задержка, чтобы процесс не завершался
            await Task.Delay(-1);
        }}

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {{
            if (update.Message is {{ Text: not null }})
            {{
                long chatId = update.Message.Chat.Id;

                Console.WriteLine(""Получено сообщение!"");
                Console.WriteLine(""Получено сообщение от чата: "" + chatId);
                // Используем новый метод SendMessage вместо SendTextMessage
                await botClient.SendMessage(chatId, ""Привет! Я ваш Telegram-бот."");
                await botClient.SendMessage(1202503239, ""Привет! Я ваш Telegram-бот."");
            }}
        }}

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {{
            Console.WriteLine($""Ошибка: {{exception.Message}}"");
            return Task.CompletedTask;
        }}
    }}
}}
";

        File.WriteAllText(Path.Combine(basePath, "Program.cs"), programCsContent);

        // 4. Пишем Dockerfile
        //    Двухэтапная сборка: сначала SDK (build), потом runtime
        //    Или можно делать self-contained, как в предыдущем примере
        string dockerfileContent = @"
# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY BotCode.csproj .
RUN dotnet restore BotCode.csproj
COPY Program.cs .
RUN dotnet publish BotCode.csproj -c Release -o /app

# Финальный образ на базе ASP.NET Core Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [""dotnet"", ""BotCode.dll""]
";
        File.WriteAllText(Path.Combine(basePath, "Dockerfile"), dockerfileContent);

        // 5. Формируем уникальный тэг (imageTag) для образа
        string imageTag = $"bot_{Guid.NewGuid().ToString("N")}";

        // 6. docker build
        //    Аргументы: build -t <imageTag> <basePath>
        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"build -t {imageTag} {basePath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
        buildProcess.Start();
        string buildOutput = await buildProcess.StandardOutput.ReadToEndAsync();
        string buildError = await buildProcess.StandardError.ReadToEndAsync();
        buildProcess.WaitForExit();

        Console.WriteLine("=== DOCKER BUILD OUTPUT ===");
        Console.WriteLine(buildOutput);
        Console.WriteLine("=== DOCKER BUILD ERROR ===");
        Console.WriteLine(buildError);

        if (buildProcess.ExitCode != 0)
        {
            throw new Exception($"docker build failed with code {buildProcess.ExitCode}");
        }

        // 7. docker run -d --name <imageTag> <imageTag>
        var runProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"run -d --name {imageTag} {imageTag}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
        runProcess.Start();
        string runOutput = await runProcess.StandardOutput.ReadToEndAsync();
        string runError = await runProcess.StandardError.ReadToEndAsync();
        runProcess.WaitForExit();

        Console.WriteLine("=== DOCKER RUN OUTPUT ===");
        Console.WriteLine(runOutput);
        Console.WriteLine("=== DOCKER RUN ERROR ===");
        Console.WriteLine(runError);

        if (runProcess.ExitCode != 0)
        {
            throw new Exception($"docker run failed with code {runProcess.ExitCode}");
        }

        // runOutput обычно возвращает ID контейнера
        string containerId = runOutput.Trim();

        Console.WriteLine($"✅ Контейнер {containerId} (образ {imageTag}) успешно запущен!");

        return containerId;
    }
}
