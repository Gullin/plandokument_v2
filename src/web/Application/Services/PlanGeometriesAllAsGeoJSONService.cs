using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Domain.Entities;

namespace Plandokument.Application.Services;

public class PlanGeometriesAllAsGeoJSONService : IPlanGeometriesAllAsGeoJSONService
{
    private readonly IPlanGeometriesAllAsGeoJSONCacheService _cacheService;

    public PlanGeometriesAllAsGeoJSONService(IPlanGeometriesAllAsGeoJSONCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<PlanGeoJSON>> GetAllAsync()
    {
        return await _cacheService.GetOrRefreshAsync();
    }
}
