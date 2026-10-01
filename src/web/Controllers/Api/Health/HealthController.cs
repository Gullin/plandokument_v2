using Microsoft.AspNetCore.Mvc;

namespace Plandokument.Controllers.Api.Health
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet("warmup")]
        public IActionResult Warmup()
        {
            return Ok();
        }
    }
}
