using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Interfaces.Repositories;

namespace Plandokument.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanRegisterBasController : ControllerBase
    {
        private readonly IPlanRegisterBasRepository _repo;

        public PlanRegisterBasController(IPlanRegisterBasRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
        {
            var result = await _repo.GetPlanRegisterBasAsync(cancellationToken);
            return Ok(result);
        }
    }
}
