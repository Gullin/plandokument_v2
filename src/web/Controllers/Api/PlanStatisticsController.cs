using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Domain.Entities;

namespace Plandokument.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class PlanStatisticsController : ControllerBase
{
    private readonly IPlanBasService _planBasService;

    public PlanStatisticsController(IPlanBasService planBasService)
    {
        _planBasService = planBasService;
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetPlanCount()
    {
        var result = await _planBasService.GetAllAsync();

        return Ok(result.Count());
    }

    [HttpGet("plantypes")]
    public async Task<IActionResult> GetPlanTypes()
    {
        // Hämta alla PlanBas
        var plans = await _planBasService.GetAllAsync();

        // Gruppéra på planfk och räkna antal i varje grupp
        var groupedCounts = plans
            .GroupBy(p => p.Register.planfk)
            .Select(g => new
            {
                PlanFk = g.Key,
                Count = g.Count()
            })
            .ToList();

        return Ok(groupedCounts);
    }

    [HttpGet("withinimplementationtime")]
    public async Task<IActionResult> GetWithinImplementationTime()
    {
        // Hämta alla PlanBas
        var plans = await _planBasService.GetAllAsync();

        return Ok(plans.Where(plan => plan.Register.isgenomf == 1).Count());
    }
}
