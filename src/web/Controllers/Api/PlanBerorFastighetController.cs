using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Domain.Entities;

namespace Plandokument.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanBerorFastighetController : ControllerBase
    {
        private readonly IPlanBerorFastighetService _planBerorFastighetService;

        public PlanBerorFastighetController(IPlanBerorFastighetService planBerorFastighetService)
        {
            _planBerorFastighetService = planBerorFastighetService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PlanBerorFastighet>>> Get()
        {
            var result = await _planBerorFastighetService.GetAllAsync();
            return Ok(result);
        }
    }
}
