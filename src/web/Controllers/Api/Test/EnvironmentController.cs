using Microsoft.AspNetCore.Mvc;

namespace Plandokument.Controllers.Api.Test
{
    [Route("api/test/[controller]")]
    [ApiController]
    public class EnvironmentController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EnvironmentController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_webHostEnvironment.EnvironmentName);
        }
    }

}
