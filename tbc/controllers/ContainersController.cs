using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using tbc.Services;

namespace tbc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContainersController : ControllerBase
    {
        private readonly IContainerService _svc;
        public ContainersController(IContainerService svc) => _svc = svc;

        // GET /api/containers
        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await _svc.GetContainersAsync());

        // POST /api/containers/{id}/start
        [HttpPost("{id}/start")]
        public async Task<IActionResult> Start(string id)
        {
            await _svc.StartContainerAsync(id);
            return NoContent();
        }

        // POST /api/containers/{id}/stop
        [HttpPost("{id}/stop")]
        public async Task<IActionResult> Stop(string id)
        {
            await _svc.StopContainerAsync(id);
            return NoContent();
        }

        // DELETE /api/containers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _svc.RemoveContainerAsync(id);
            return NoContent();
        }
    }
}
