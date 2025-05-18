// Controllers/SchemasController.cs
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBC.Data;     
using TBC.Models.Entities;

namespace TBC.Controllers
{
    [ApiController]
    [Route("api/bots/{botId:int}/schemas")]
    public class SchemasController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SchemasController(AppDbContext db) => _db = db;

        // GET api/bots/schemas
        [HttpGet]
        public async Task<IActionResult> ListSchemas(int botId)
        {
            if (!await _db.BotInstances.AnyAsync(b => b.Id == botId))
                return NotFound(new { error = "Bot not found" });

            var list = await _db.BotSchemas
                .Where(s => s.BotInstanceId == botId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    s.Id,
                    s.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET api/bots/schemas/
        // отдаёт чисто JSON схемы
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSchema(int botId, int id)
        {
            var schema = await _db.BotSchemas
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.BotInstanceId == botId);

            if (schema == null)
                return NotFound();

            return Content(schema.Content, "application/json");
        }

        // POST api/bots/schemas
        // сохраняет новую схему
        [HttpPost]
        public async Task<IActionResult> PostSchema(
            int botId,
            [FromBody] JsonDocument schemaJson)
        {
            if (!await _db.BotInstances.AnyAsync(b => b.Id == botId))
                return NotFound(new { error = "Bot not found" });

            var raw = schemaJson.RootElement.GetRawText();

            var entity = new BotSchema
            {
                BotInstanceId = botId,
                Content = raw,
                CreatedAt = DateTime.UtcNow
            };
            _db.BotSchemas.Add(entity);
            await _db.SaveChangesAsync();

            // вернём 201 + метаданные новой записи
            return CreatedAtAction(
                nameof(GetSchema),
                new { botId, id = entity.Id },
                new { entity.Id, entity.CreatedAt });
        }
    }
}
