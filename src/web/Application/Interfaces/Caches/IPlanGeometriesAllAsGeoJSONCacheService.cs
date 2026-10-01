using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Caches;

public interface IPlanGeometriesAllAsGeoJSONCacheService
{
    Task<IEnumerable<PlanGeoJSON>> GetOrRefreshAsync();
    void ClearCache();
}
