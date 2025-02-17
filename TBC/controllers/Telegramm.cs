/*using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Diagnostics;

class DynamicCompiler
{
    public static async Task StartBot()
    {
        string botCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

class TelegramBot
{
    public static async Task Main()
    {
        var botClient = new TelegramBotClient(""7832218363:AAFiZxbzmmYa5WxARS6HY8GH_AqUgazGU0k"");

        await botClient.DeleteWebhookAsync();
        botClient.StartReceiving(UpdateHandler, ErrorHandler);

        Console.WriteLine(""Бот запущен."");
        Console.ReadLine();
    }

    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Text: not null })
        {
            long chatId = update.Message.Chat.Id;
            await botClient.SendTextMessageAsync(chatId, ""Привет! Я ваш Telegram-бот."");
        }
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($""Ошибка: {exception.Message}"");
        return Task.CompletedTask;
    }
}";

        string filePath = "GeneratedBot.cs";
        File.WriteAllText(filePath, botCode);
        Console.WriteLine($"Файл {filePath} создан.");

        // Компиляция кода
        string outputExe = "GeneratedBot";
        bool success = CompileCode(filePath, outputExe);

        if (success)
        {
            Console.WriteLine($"Файл {outputExe} скомпилирован.");
            RunCompiledExe(outputExe);
        }
        else
        {
            Console.WriteLine("Ошибка компиляции!");
        }
    }

    static bool CompileCode(string inputFilePath, string outputExe)
    {
        try
        {
            Console.WriteLine($"🧹 Очищаем старые файлы...");
            string outputFile = "/app/GeneratedBot"; // Без .exe, т.к. Linux

            if (File.Exists(outputFile)) File.Delete(outputFile);

            Console.WriteLine($"✅ Старые файлы удалены.");
            Console.WriteLine($"📂 Проверяем путь: {inputFilePath}");

            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine($"❌ Ошибка: Файл `{inputFilePath}` не найден! Компиляция невозможна.");
                return false;
            }

            Console.WriteLine($"📜 Читаем исходный код...");
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(inputFilePath));

            Console.WriteLine($"🔗 Подключаем зависимости...");
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && File.Exists(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();

            references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Console).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Task).Assembly.Location));

            string telegramBotDll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Telegram.Bot.dll");
            if (File.Exists(telegramBotDll))
            {
                references.Add(MetadataReference.CreateFromFile(telegramBotDll));
                Console.WriteLine("✅ Подключена библиотека Telegram.Bot.dll");
            }
            else
            {
                Console.WriteLine("❌ Ошибка: Telegram.Bot.dll не найдена!");
                return false;
            }

            Console.WriteLine($"⚙️ Начинаем компиляцию под Linux...");

            // 🔥 Указываем, что код должен собираться в Linux-совместимый бинарник
            var options = new CSharpCompilationOptions(
                OutputKind.ConsoleApplication,
                platform: Platform.AnyCpu, // <--- Делаем код платформонезависимым
                optimizationLevel: OptimizationLevel.Release
            );

            var compilation = CSharpCompilation.Create("GeneratedBot")
                .WithOptions(options)
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTree);



            using (var fs = new FileStream(outputFile, FileMode.Create))
            {
                EmitResult result = compilation.Emit(fs);

                if (!result.Success)
                {
                    Console.WriteLine("❌ Ошибка компиляции:");
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        Console.WriteLine($" - {diagnostic}");
                    }
                    return false;
                }
            }

            Console.WriteLine($"✅ Файл `{outputFile}` успешно скомпилирован.");

            // 📂 Создаем временный проект (если его нет)
            string projectPath = "/app/GeneratedBotProject";
            string projectFile = Path.Combine(projectPath, "GeneratedBot.csproj");

            // Проверяем, есть ли уже csproj, если нет — создаем его
            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
                File.WriteAllText(projectFile, @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
    <SelfContained>true</SelfContained>
  </PropertyGroup>
</Project>");
                Console.WriteLine($"✅ Файл проекта создан: {projectFile}");
            }

            // 📦 Запускаем dotnet publish для создания ELF-файла
            Console.WriteLine("📦 Публикуем self-contained бинарник...");
            var publishProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"publish \"{projectPath}\" -r linux-x64 -c Release --self-contained true -o /app",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            publishProcess.Start();
            publishProcess.WaitForExit();
            Console.WriteLine($"✅ Публикация завершена с кодом {publishProcess.ExitCode}");

            Console.WriteLine($"📦 Проверяем лог публикации...");
            if (File.Exists("/app/publish_log.txt"))
            {
                Console.WriteLine(File.ReadAllText("/app/publish_log.txt"));
            }
            return true;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка в CompileCode(): {ex.Message}");
            return false;
        }
    }


    static void RunCompiledExe(string exePath)
    {
        try
        {
            if (!File.Exists(exePath))
            {
                Console.WriteLine($"❌ Ошибка: Файл {exePath} не найден! Проверьте, правильно ли он скомпилировался.");
                return;
            }

            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                Process.Start("chmod", $"+x {exePath}")?.WaitForExit();
            }

            Console.WriteLine($"🚀 Запуск {exePath}...");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false
                }
            };

            Console.WriteLine($"🚀 Создан новый процесс {exePath}...");

            process.OutputDataReceived += (sender, args) => Console.WriteLine($"[Вывод]: {args.Data}");
            process.ErrorDataReceived += (sender, args) => Console.WriteLine($"[Ошибка]: {args.Data}");
            Console.WriteLine($"🚀 Проверяем пути к {exePath}...");

            if (Directory.Exists("/app"))
            {
                Console.WriteLine($"✅ Директория /app найдена  {exePath}...");
                if (File.Exists("/app/GeneratedBot"))
                {
                    Console.WriteLine($"✅ Файл {exePath} найден...");

                    process.Start();
                }

                else
                    Console.WriteLine($"❌ Ошибка: файл {exePath} не найден  ...");
            }

            else
                Console.WriteLine($"❌ Ошибка: дирректория /app не найдена  {exePath}...");

            Console.WriteLine($"🚀 Процесс \"старт\" {exePath}...");

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            Console.WriteLine($"✅ Процесс завершен с кодом {process.ExitCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка запуска: {ex.Message}");
        }
    }

}

*/