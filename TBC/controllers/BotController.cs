// Controllers/BotsController.cs
using Microsoft.AspNetCore.Mvc;
using TBC.Models;
using TBC.Services;

namespace TBC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BotsController : ControllerBase
    {
        private readonly IDockerBotBuilder _builder;

        public BotsController(IDockerBotBuilder builder)
        {
            _builder = builder;
        }

        // POST api/bots/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateBotRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.TelegramToken))
                return BadRequest(new { error = "BotToken обязателен" });

            try
            {
                string containerId = await _builder.CreateAndRunBot(
                    req.TelegramToken,
                    req.BotCode,
                    req.BotProj,
                    req.BotDocker
                );

                return Ok(new { containerId });
            }
            catch (Exception ex)
            {
                // TODO: логировать ex
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
