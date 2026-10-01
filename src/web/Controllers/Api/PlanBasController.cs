using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Domain.Entities;

namespace Plandokument.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanBasController : ControllerBase
    {
        private readonly IPlanBasService _planBasService;

        public PlanBasController(IPlanBasService planBasService)
        {
            _planBasService = planBasService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PlanBas>>> Get()
        {
            var result = await _planBasService.GetAllAsync();
            return Ok(result);
        }
    }
}
