using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TBC.Data;
using TBC.Models.DTO;
using TBC.Models.Entities;
using TBC.Models.Requests;

namespace TBC.Services
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
            await _db.SaveChangesAsync();

            // пересобираем и рестартим
            await RestartContainerAsync(bot, null, null, null, initialStatus: "Running");
            return ToDto(bot);
        }

        // —————————————————————————————————————————————————

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
            // 1) остановить старый контейнер
            if (!string.IsNullOrEmpty(bot.ContainerId))
            {
                try { await _builder.StopAndRemoveBot(bot.ContainerId); }
                catch { /* лог */ }
                bot.ContainerId = null;
            }

            bot.Status = initialStatus;
            await _db.SaveChangesAsync();

            // 2) подготовить шаблоны
            var (code, proj, docker) = PrepareTemplates(bot.Token, userCode, userProj, userDocker);

            // 3) собрать и запустить
            try
            {
                var newId = await _builder.CreateAndRunBot(bot.Token, code, proj, docker);
                bot.ContainerId = newId;
                bot.Status = "Running";
            }
            catch
            {
                bot.Status = "Error";
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Возвращает тройку (code, proj, docker), беря пользовательские куски если есть,
        /// иначе читая файлы-­шаблоны из папки.
        /// </summary>
        private (string? code, string? proj, string? docker) PrepareTemplates(
            string token,
            string? userCode,
            string? userProj,
            string? userDocker)
        {
            string LoadTpl(string name)
                => File.ReadAllText(Path.Combine(_templatesPath, name));

            // BotCode.cs.tpl должен содержать {{TelegramToken}}
            string defaultCode = LoadTpl("BotCode.cs.tpl").Replace("{{TelegramToken}}", token);
            string defaultProj = LoadTpl("BotProj.csproj.tpl");
            string defaultDocker = LoadTpl("Dockerfile.tpl");

            return (
                userCode ?? defaultCode,
                userProj ?? defaultProj,
                userDocker ?? defaultDocker
            );
        }
    }
}
