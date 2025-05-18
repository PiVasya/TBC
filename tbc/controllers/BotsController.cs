using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using tbc.Models.DTO;
using tbc.Models.Requests;
using tbc.Services;

namespace tbc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BotsController : ControllerBase
    {
        private readonly IBotService _botService;

        public BotsController(IBotService botService)
            => _botService = botService;

        // POST /api/bots
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBotRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(new { error = "Name is required" });
            if (string.IsNullOrWhiteSpace(req.TelegramToken))
                return BadRequest(new { error = "TelegramToken is required" });

            var dto = await _botService.CreateAsync(req);
            return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
        }

        // GET /api/bots/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var dto = await _botService.GetByIdAsync(id);
            return dto is null ? NotFound() : Ok(dto);
        }

        // GET /api/bots
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var list = await _botService.ListAsync();
            return Ok(list);
        }

        // PUT /api/bots/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBotRequest req)
        {
            var dto = await _botService.UpdateAsync(id, req);
            return dto is null
                ? NotFound()
                : Ok(dto);
        }

        // DELETE /api/bots/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _botService.DeleteAsync(id);
            return deleted
                ? NoContent()
                : NotFound(new { error = $"Bot with id={id} not found" });
        }

        // POST /api/bots/{id}/start
        [HttpPost("{id:int}/start")]
        public async Task<IActionResult> Start(int id)
        {
            var started = await _botService.StartAsync(id);
            return started
                ? NoContent()
                : NotFound(new { error = $"Bot with id={id} not found or failed to start" });
        }

        // POST /api/bots/{id}/stop
        [HttpPost("{id:int}/stop")]
        public async Task<IActionResult> Stop(int id)
        {
            var stopped = await _botService.StopAsync(id);
            return stopped
                ? NoContent()
                : NotFound(new { error = $"Bot with id={id} not found or failed to stop" });
        }

        // POST /api/bots/{id}/rebuild
        [HttpPost("{id:int}/rebuild")]
        public async Task<IActionResult> Rebuild(int id, [FromBody] UpdateBotRequest req)
        {
            // при rebuild мы перерасписываем токен/имя, пересоздаём контейнер
            var dto = await _botService.RebuildAsync(id, req);
            return dto is null
                ? NotFound(new { error = $"Bot with id={id} not found or failed to rebuild" })
                : Ok(dto);
        }
    }
}
