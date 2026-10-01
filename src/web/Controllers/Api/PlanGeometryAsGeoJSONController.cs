using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Interfaces.Repositories;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Domain.Entities;
using System.Numerics;

namespace Plandokument.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanGeometryAsGeoJSONController : ControllerBase
    {
        private readonly IPlanGeometriesAllAsGeoJSONService _planGeometriesAllAsGeoJSONService;
        private readonly IPlanGeometryBySearchAsGeoJSONRepository _planGeometryBySearchAsGeoJSONRepository;

        public PlanGeometryAsGeoJSONController(IPlanGeometriesAllAsGeoJSONService planGeometriesAllAsGeoJSONService, IPlanGeometryBySearchAsGeoJSONRepository planGeometryBySearchAsGeoJSONRepository)
        {
            _planGeometriesAllAsGeoJSONService = planGeometriesAllAsGeoJSONService;
            _planGeometryBySearchAsGeoJSONRepository = planGeometryBySearchAsGeoJSONRepository;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<PlanGeoJSON>>> Get()
        {
            var result = await _planGeometriesAllAsGeoJSONService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("filterbykvp")] // Example: /api/plangeometryasgeojson/filterbykvp?planids=1&planids=2&planids=3
        public async Task<ActionResult<PlanGeoJSON>> FilterByKVP([FromQuery] List<int> planIds)
        {
            var result = await _planGeometryBySearchAsGeoJSONRepository.GetPlanGeometryBySearchAsGeoJSONAsync(
                    planIds.Select(id => id.ToString()).ToList()
                );
            return Ok(result);
        }

        [HttpPost("filterbyjson")] // Example: /api/plangeometryasgeojson/filterbyjson
        public async Task<ActionResult<PlanGeoJSON>> FilterByJSON([FromBody] List<int> planIds)
        {
            var result = await _planGeometryBySearchAsGeoJSONRepository.GetPlanGeometryBySearchAsGeoJSONAsync(
                    planIds.Select(id => id.ToString()).ToList()
                );
            return Ok(result);
        }

        [HttpPost("single/{planId}")]
        public async Task<ActionResult<PlanGeoJSON>> Get(int planId)
        {
            var result = await _planGeometryBySearchAsGeoJSONRepository.GetPlanGeometryBySearchAsGeoJSONAsync(
                    new List<string> { planId.ToString() }
                );
            return Ok(result);
        }
    }
}
