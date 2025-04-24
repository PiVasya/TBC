using System.Diagnostics;    // для ProjCs, BotCs, DockerCs
using TBC.Services;      // для IDockerBotBuilder

namespace TBC.Services
{
    public class DockerBotBuilder : IDockerBotBuilder
    {
        private readonly string _tmplDir;

        public DockerBotBuilder(IWebHostEnvironment env)
        {
            // Templates лежат в корне контента приложения
            _tmplDir = Path.Combine(env.ContentRootPath, "Templates");
        }

        public async Task<string> CreateAndRunBot(
            string telegramToken,
            string? botCode,
            string? botProj,
            string? botDocker)
        {
            // 1. Путь для файлов
            string folderName = "bot_" + Guid.NewGuid().ToString("N");
            string basePath = Path.Combine("/tmp", folderName);
            Directory.CreateDirectory(basePath);

            // 2. Загружаем и сохраняем csproj
            string projTemplate = File.ReadAllText(Path.Combine(_tmplDir, "BotProj.csproj.tpl"));
            string projText = string.IsNullOrWhiteSpace(botProj)
                ? projTemplate
                : botProj;
            File.WriteAllText(Path.Combine(basePath, "BotCode.csproj"), projText);

            // 3. Загружаем и сохраняем Program.cs
            string codeTemplate = File.ReadAllText(Path.Combine(_tmplDir, "BotCode.cs.tpl"));
            // Подставляем {{TelegramToken}} в шаблон
            codeTemplate = codeTemplate.Replace("{{TelegramToken}}", telegramToken);
            string codeText = string.IsNullOrWhiteSpace(botCode)
                ? codeTemplate
                : botCode;
            File.WriteAllText(Path.Combine(basePath, "Program.cs"), codeText);

            // 4. Загружаем и сохраняем Dockerfile
            string dockerTemplate = File.ReadAllText(Path.Combine(_tmplDir, "Dockerfile.tpl"));
            string dockerText = string.IsNullOrWhiteSpace(botDocker)
                ? dockerTemplate
                : botDocker;
            File.WriteAllText(Path.Combine(basePath, "Dockerfile"), dockerText);

            // 5. Формируем imageTag
            string imageTag = $"bot_{Guid.NewGuid():N}";

            // 6. Собираем образ
            await RunProcessOrThrow("docker",
                $"build -t {imageTag} {basePath}");

            // 7. Запускаем контейнер
            string containerId =
                (await RunProcessOrThrow("docker",
                  $"run -d --name {imageTag} {imageTag}"))
                .Trim();

            return containerId;
        }

        private static async Task<string> RunProcessOrThrow(
            string fileName, string arguments)
        {
            var psi = new ProcessStartInfo(fileName, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            var proc = Process.Start(psi)!;
            string outp = await proc.StandardOutput.ReadToEndAsync();
            string err = await proc.StandardError.ReadToEndAsync();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
                throw new Exception(
                    $"{fileName} {arguments} завершился с кодом {proc.ExitCode}:\n{err}"
                );

            return outp;
        }
    }
}
