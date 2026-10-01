using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Domain.Entities;

namespace Plandokument.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanBerorPlanController : ControllerBase
    {
        private readonly IPlanBerorPlanService _planBerorPlanService;

        public PlanBerorPlanController(IPlanBerorPlanService planBerorPlanService)
        {
            _planBerorPlanService = planBerorPlanService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PlanBerorPlan>>> Get()
        {
            var result = await _planBerorPlanService.GetAllAsync();
            return Ok(result);
        }


        [HttpGet("{planIds}")]
        public async Task<ActionResult<List<PlanBerorPlan>>> GetBySpecificPlans(string planIds)
        {
            var planIdList = planIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var result = await _planBerorPlanService.GetBySpecificPlansAsync(planIdList);
            return Ok(result);
        }
    }
}
