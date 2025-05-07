using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.IO;
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
            // Cчитаем, что папка Templates лежит в content root приложения
            _templatesPath = Path.Combine(env.ContentRootPath, "Templates");
        }

        public async Task<IEnumerable<BotDto>> ListAsync()
        {
            return await _db.BotInstances
                .AsNoTracking()
                .OrderBy(b => b.Id)
                .Select(b => new BotDto(
                    b.Id,
                    b.Name,
                    b.Token,
                    b.ContainerId,
                    b.Status,
                    b.CreatedAt
                ))
                .ToListAsync();
        }

        public async Task<BotDto> CreateAsync(CreateBotRequest req)
        {
            // 1) создаём запись бота в БД
            var bot = new BotInstance
            {
                Name = req.Name,
                Token = req.TelegramToken,
                Status = "Creating",
                CreatedAt = DateTime.UtcNow
            };
            _db.BotInstances.Add(bot);
            await _db.SaveChangesAsync();  // чтобы EF присвоил bot.Id

            // 2) сохраняем первую версию схемы
            var rawSchema = req.Schema.GetRawText();
            var schemaEnt = new BotSchema
            {
                BotInstanceId = bot.Id,
                Content = rawSchema,
                CreatedAt = DateTime.UtcNow
            };
            _db.BotSchemas.Add(schemaEnt);
            await _db.SaveChangesAsync();

            // 3) получаем код/проект/докер из запроса или шаблонов
            var code = !string.IsNullOrWhiteSpace(req.BotCode)
                        ? req.BotCode
                        : File.ReadAllText(Path.Combine(_templatesPath, "BotCode.cs.tpl"));
            var proj = !string.IsNullOrWhiteSpace(req.BotProj)
                        ? req.BotProj
                        : File.ReadAllText(Path.Combine(_templatesPath, "BotProj.csproj.tpl"));
            var docker = !string.IsNullOrWhiteSpace(req.BotDocker)
                        ? req.BotDocker
                        : File.ReadAllText(Path.Combine(_templatesPath, "Dockerfile.tpl"));

            // 4) запускаем контейнер (этот блок оставлен без изменений)
            try
            {
                var containerId = await _builder.CreateAndRunBot(
                    bot.Token, code, proj, docker);

                bot.ContainerId = containerId;
                bot.Status = "Running";
            }
            catch
            {
                bot.Status = "Error";
            }

            await _db.SaveChangesAsync();

            // 5) возвращаем DTO
            return new BotDto(
                bot.Id,
                bot.Name,
                bot.Token,
                bot.ContainerId,
                bot.Status,
                bot.CreatedAt
            );
        }

        public async Task<BotDto?> UpdateAsync(int id, UpdateBotRequest req)
        {
            // находим запись
            var bot = await _db.BotInstances.FindAsync(id);
            if (bot == null)
                return null;

            // сохраняем только имя и токен
            bot.Name = req.Name;
            bot.Token = req.TelegramToken;

            await _db.SaveChangesAsync();

            return new BotDto(
                bot.Id,
                bot.Name,
                bot.Token,
                bot.ContainerId,
                bot.Status,
                bot.CreatedAt
            );
        }

        public async Task<BotDto?> GetByIdAsync(int id)
        {
            var b = await _db.BotInstances
                             .AsNoTracking()
                             .FirstOrDefaultAsync(x => x.Id == id);
            if (b is null) return null;
            return new BotDto(b.Id, b.Name, b.Token, b.ContainerId, b.Status, b.CreatedAt);
        }
    }
}
