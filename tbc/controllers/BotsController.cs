using Microsoft.AspNetCore.Mvc;
using TBC.Models.DTO;
using TBC.Models.Requests;
using TBC.Services;

namespace TBC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BotsController : ControllerBase
    {
        private readonly IBotService _botService;
        public BotsController(IBotService botService)
            => _botService = botService;

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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var dto = await _botService.GetByIdAsync(id);
            return dto is null ? NotFound() : Ok(dto);
        }

        // GET api/bots
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var list = await _botService.ListAsync();
            return Ok(list);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBotRequest req)
        {
            var dto = await _botService.UpdateAsync(id, req);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _botService.DeleteAsync(id);
            if (!deleted) return NotFound(new { message = $"Bot with id={id} not found" });
            return NoContent(); // 204
        }
    }
}
