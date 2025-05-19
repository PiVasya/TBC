using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using tbc.Data;
using tbc.Models.DTO;
using tbc.Models.Entities;
using tbc.Models.Requests;
using tbc.Services.CodeGen;

namespace tbc.Services
{
    public class BotService : IBotService
    {
        private readonly AppDbContext _db;
        private readonly IDockerBotBuilder _builder;
        private readonly string _templatesPath;

        public BotService(AppDbContext db,
                          IDockerBotBuilder builder,
                          IWebHostEnvironment env)
        {
            _db = db;
            _builder = builder;
            _templatesPath = Path.Combine(env.ContentRootPath, "Templates");
        }

        public Task<IEnumerable<BotDto>> ListAsync() =>
            _db.BotInstances
               .AsNoTracking()
               .OrderBy(b => b.Id)
               .Select(b => new BotDto(b.Id, b.Name, b.Token, b.ContainerId, b.Status, b.CreatedAt))
               .ToListAsync()
               .ContinueWith(t => (IEnumerable<BotDto>)t.Result);

        public async Task<BotDto> CreateAsync(CreateBotRequest req)
        {
            // 1) создаём запись
            var bot = new BotInstance
            {
                Name = req.Name,
                Token = req.TelegramToken,
                Status = "Creating",
                CreatedAt = DateTime.UtcNow
            };
            _db.BotInstances.Add(bot);
            await _db.SaveChangesAsync();  // bot.Id появился

            // 2) сохраняем схему
            _db.BotSchemas.Add(new BotSchema
            {
                BotInstanceId = bot.Id,
                Content = req.Schema.GetRawText(),
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            // 3) стартуем контейнер
            await RestartContainerAsync(bot, req.BotCode, req.BotProj, req.BotDocker, initialStatus: "Running");

            return ToDto(bot);
        }

        public async Task<BotDto?> UpdateAsync(int id, UpdateBotRequest req)
        {
            var bot = await _db.BotInstances.FindAsync(id);
            if (bot is null) return null;

            bot.Name = req.Name;
            bot.Token = req.TelegramToken;
            await _db.SaveChangesAsync();
            return ToDto(bot);
        }

        public Task<BotDto?> GetByIdAsync(int id) =>
            _db.BotInstances
               .AsNoTracking()
               .Where(b => b.Id == id)
               .Select(b => new BotDto(b.Id, b.Name, b.Token, b.ContainerId, b.Status, b.CreatedAt))
               .FirstOrDefaultAsync();

        public async Task<bool> DeleteAsync(int id)
        {
            var bot = await _db.BotInstances.FindAsync(id);
            if (bot is null) return false;

            if (!string.IsNullOrEmpty(bot.ContainerId))
            {
                try { await _builder.StopAndRemoveBot(bot.ContainerId); }
                catch { /* логируем */ }
            }

            _db.BotInstances.Remove(bot);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> StopAsync(int id)
        {
            var bot = await _db.BotInstances.FindAsync(id);
            if (bot is null || string.IsNullOrEmpty(bot.ContainerId)) return false;

            try
            {
                await _builder.StopAndRemoveBot(bot.ContainerId);
                bot.Status = "Stopped";
                bot.ContainerId = null;
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> StartAsync(int id)
        {
            var bot = await _db.BotInstances.FindAsync(id);
            if (bot is null) return false;

            await RestartContainerAsync(bot, null, null, null, initialStatus: "Running");
            return bot.Status == "Running";
        }

        public async Task<BotDto?> RebuildAsync(int id, UpdateBotRequest req)
        {
            var bot = await _db.BotInstances.FindAsync(id);
            if (bot is null) return null;

            // обновляем поля
            bot.Name = req.Name;
            bot.Token = req.TelegramToken;
            bot.AdminId = req.AdminId;
            await _db.SaveChangesAsync();

            // пересобираем и рестартим
            await RestartContainerAsync(bot, null, null, null, initialStatus: "Running");
            return ToDto(bot);
        }


        private BotDto ToDto(BotInstance b)
            => new BotDto(b.Id, b.Name, b.Token, b.ContainerId, b.Status, b.CreatedAt);

        /// <summary>
        /// Универсальный метод: останавливает старый контейнер, собирает+запускает новый и сохраняет статус.
        /// </summary>
        private async Task RestartContainerAsync(
            BotInstance bot,
            string? userCode,
            string? userProj,
            string? userDocker,
            string initialStatus)
        {
            Console.WriteLine($"[BotService] === RestartContainerAsync start for BotId={bot.Id} ===");
            Console.WriteLine($"[BotService] templatesPath = {_templatesPath}");

            // 1) остановить старый контейнер
            if (!string.IsNullOrEmpty(bot.ContainerId))
            {
                Console.WriteLine($"[BotService] Stopping old container: {bot.ContainerId}");
                try { await _builder.StopAndRemoveBot(bot.ContainerId); }
                catch (Exception ex) { Console.WriteLine($"[BotService] WARNING: StopAndRemove failed: {ex.Message}"); }
                bot.ContainerId = null;
            }

            // 2) пометить статус
            bot.Status = initialStatus;
            await _db.SaveChangesAsync();
            Console.WriteLine($"[BotService] Status set to '{initialStatus}' in DB");

            // 3) загрузить схему
            var schemaJson = await _db.BotSchemas
                .AsNoTracking()
                .Where(s => s.BotInstanceId == bot.Id)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => s.Content)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Schema not found");
            Console.WriteLine($"[BotService] Loaded schema JSON length={schemaJson.Length}");

            // 4) очистить схему
            var root = JToken.Parse(schemaJson);
            var clean = new JObject(
                new JProperty("nodes", new JArray(root["nodes"]!
                    .Select(n => new JObject(
                        new JProperty("id", n["id"]!),
                        new JProperty("type", n["type"]!),
                        new JProperty("data", n["data"]!)
                    )))),
                new JProperty("edges", new JArray(root["edges"]!
                    .Select(e => new JObject(
                        new JProperty("source", e["source"]!),
                        new JProperty("target", e["target"]!)
                    ))))
            );
            var cleanJson = clean.ToString(Formatting.None);
            Console.WriteLine($"[BotService] Clean JSON length={cleanJson.Length}");
            Console.WriteLine($"[BotService] AdminId = {Convert.ToInt64(bot.AdminId)}");
            // 5) генерируем код
            Console.WriteLine($"[BotService] Calling SchemaCodeGenerator.GenerateCode...");
            var (generatedCode, generatedProj, generatedDocker) =
                SchemaCodeGenerator.GenerateCode(
                    schemaJson: cleanJson,
                    telegramToken: bot.Token,
                    adminChatId: Convert.ToInt64(bot.AdminId),
                    templatesPath: _templatesPath);
            Console.WriteLine($"[BotService] GeneratedCode length={generatedCode.Length}");
            Console.WriteLine($"[BotService] GeneratedProj length={generatedProj.Length}");
            Console.WriteLine($"[BotService] GeneratedDocker length={generatedDocker.Length}");

            // 6) что пойдёт в контейнер
            var codeToUse = string.IsNullOrWhiteSpace(userCode) ? generatedCode : userCode!;
            var projToUse = string.IsNullOrWhiteSpace(userProj) ? generatedProj : userProj!;
            var dockerToUse = string.IsNullOrWhiteSpace(userDocker) ? generatedDocker : userDocker!;
            Console.WriteLine($"[BotService] Final code length={codeToUse.Length}, proj length={projToUse.Length}, docker length={dockerToUse.Length}");

            // 7) билд и запуск
            try
            {
                Console.WriteLine($"[BotService] Invoking DockerBotBuilder.CreateAndRunBot...");
                var newId = await _builder.CreateAndRunBot(
                    bot.Token,
                    botCode: codeToUse,
                    botProj: projToUse,
                    botDocker: dockerToUse);
                bot.ContainerId = newId;
                bot.Status = "Running";
                Console.WriteLine($"[BotService] New container started: {newId}");
            }
            catch (Exception ex)
            {
                bot.Status = "Error";
                Console.WriteLine($"[BotService] ERROR: CreateAndRunBot failed: {ex.Message}");
            }

            await _db.SaveChangesAsync();
            Console.WriteLine($"[BotService] === RestartContainerAsync end for BotId={bot.Id} ===");
        }
    }
}
