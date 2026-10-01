using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Interfaces.Repositories;

namespace Plandokument.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanGeometriBasController : ControllerBase
    {
        private readonly IPlanGeometriBasRepository _repo;
        private readonly ILogger _logger;

        //public PlanGeometriBasController(IPlanGeometriBasRepository repo) => _repo = repo;
        public PlanGeometriBasController(IPlanGeometriBasRepository repo, ILogger logger)
        {
            _repo = repo;
            _logger = logger;

            _logger.LogInformation("PlanGeometriBasController initialized with repository {RepoType}", repo.GetType().Name);
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
        {
            var result = await _repo.GetPlanGeometriBasAsync(cancellationToken);
            _logger.LogInformation("Retrieved {Count} PlanGeometriBas records", result.Count());
            return Ok(result);
        }
    }
}
