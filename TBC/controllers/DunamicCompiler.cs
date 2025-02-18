using System.Diagnostics;
using TBC.controllers;

public static class DockerBotBuilder
{
    public static async Task<string> CreateAndRunBot(string telegramToken, string BotCode, string BotProj, string BotDocker)
    {
        // 1. Создаём уникальную папку для файлов
        //    Например: /tmp/bot_XXXXXXXX
        string folderName = "bot_" + Guid.NewGuid().ToString("N");
        string basePath = Path.Combine("/tmp", folderName);
        Directory.CreateDirectory(basePath);

        // 2. Пишем .csproj
        //    Включаем ссылку на Telegram.Bot
        //    Авто-компиляция *.cs (EnableDefaultCompileItems=true по умолчанию)
        BotProj = string.IsNullOrWhiteSpace(BotProj) ? ProjCs.StdProj : BotProj;

        File.WriteAllText(Path.Combine(basePath, "BotCode.csproj"), BotProj);

        // 3. Пишем Program.cs
        //    Вместо "Нажмите Enter..." делаем бесконечное ожидание,
        //    чтобы бот жил, пока контейнер не будет остановлен.

        BotCode = string.IsNullOrWhiteSpace(BotCode) ? BotCs.StdCode(telegramToken) : BotCode;

        File.WriteAllText(Path.Combine(basePath, "Program.cs"), BotCode);

        // 4. Пишем Dockerfile
        //    Двухэтапная сборка: сначала SDK (build), потом runtime
        //    Или можно делать self-contained, как в предыдущем примере
        BotDocker = string.IsNullOrWhiteSpace(BotDocker) ? DockerCs.StdDocker : BotDocker;

        File.WriteAllText(Path.Combine(basePath, "Dockerfile"), BotDocker);

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
