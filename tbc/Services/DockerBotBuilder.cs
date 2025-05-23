using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace tbc.Services
{
    public class DockerBotBuilder : IDockerBotBuilder
    {
        private readonly string _tmplDir;
        private readonly string _networkName;

        public DockerBotBuilder(IWebHostEnvironment env)
        {
            _tmplDir = Path.Combine(env.ContentRootPath, "Templates");
            Console.WriteLine($"[DockerBotBuilder] Templates directory: {_tmplDir}");

            var composeProject =
                Environment.GetEnvironmentVariable("COMPOSE_PROJECT_NAME")
                ?? "tbc";
            _networkName = $"{composeProject}_default";
            Console.WriteLine($"[DockerBotBuilder] Using compose network: {_networkName}");
        }

        public async Task<string> CreateAndRunBot(
            string botCode,
            string botProj,
            string botDocker)
        {

            // 1) Папка для проекта
            var folderName = "bot_" + Guid.NewGuid().ToString("N");
            var basePath = Path.Combine("/tmp", folderName);
            Directory.CreateDirectory(basePath);
            Console.WriteLine($"[DockerBotBuilder] Working dir: {basePath}");

            // 2) csproj: если user-proj не пустой — используем его, иначе читаем шаблон
            var projPath = Path.Combine(basePath, "BotCode.csproj");
            File.WriteAllText(projPath, botProj);
            Console.WriteLine($"[DockerBotBuilder] Wrote csproj to {projPath} (length={botProj.Length})");

            
            
            var codePath = Path.Combine(basePath, "Program.cs");
            File.WriteAllText(codePath, botCode);
            Console.WriteLine($"[DockerBotBuilder] Wrote Program.cs to {codePath} (length={botCode.Length})");

            // 4) Dockerfile: если user-docker не пустой — используем его, иначе читаем шаблон
            var dfPath = Path.Combine(basePath, "Dockerfile");
            File.WriteAllText(dfPath, botDocker);
            Console.WriteLine($"[DockerBotBuilder] Wrote Dockerfile to {dfPath} (length={botDocker.Length})");

            // 5) Тэг образа
            var imageTag = $"bot_{Guid.NewGuid():N}";
            Console.WriteLine($"[DockerBotBuilder] Image tag = {imageTag}");

            // 6) Сборка Docker-образа
            Console.WriteLine($"[DockerBotBuilder] Building image…");
            var buildOut = await RunProcessOrThrow("docker", $"build -t {imageTag} {basePath}");
            Console.WriteLine($"[DockerBotBuilder] Build output:\n{buildOut}");

            // 7) Запуск контейнера в сети Compose
            Console.WriteLine($"[DockerBotBuilder] Running container in network '{_networkName}'…");
            var runOut = await RunProcessOrThrow(
                "docker",
                $"run -d --name {imageTag} --network {_networkName} {imageTag}"
            );
            var containerId = runOut.Trim();
            Console.WriteLine($"[DockerBotBuilder] Run output (containerId): {containerId}");
            Console.WriteLine($"[DockerBotBuilder] === CreateAndRunBot end ===");

            return containerId;
        }

        public async Task StopAndRemoveBot(string containerId)
        {
            Console.WriteLine($"[DockerBotBuilder] Stopping container {containerId}");
            await RunProcessOrThrow("docker", $"stop {containerId}");
            Console.WriteLine($"[DockerBotBuilder] Removing container {containerId}");
            await RunProcessOrThrow("docker", $"rm {containerId}");
        }

        private static async Task<string> RunProcessOrThrow(string fileName, string arguments)
        {
            Console.WriteLine($"[DockerBotBuilder] >>> {fileName} {arguments}");
            var psi = new ProcessStartInfo(fileName, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            using var proc = Process.Start(psi)!;
            var outp = await proc.StandardOutput.ReadToEndAsync();
            var err = await proc.StandardError.ReadToEndAsync();
            proc.WaitForExit();
            Console.WriteLine($"[DockerBotBuilder] <<< exitCode={proc.ExitCode}");
            if (!string.IsNullOrEmpty(outp)) Console.WriteLine($"[DockerBotBuilder] STDOUT:\n{outp}");
            if (!string.IsNullOrEmpty(err)) Console.WriteLine($"[DockerBotBuilder] STDERR:\n{err}");
            if (proc.ExitCode != 0)
                throw new Exception($"{fileName} {arguments} failed with code {proc.ExitCode}");
            return outp;
        }
    }
}
