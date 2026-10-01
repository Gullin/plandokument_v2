using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Caches;

public interface IPlanBerorPlanCacheService
{
    Task<IEnumerable<PlanBerorPlan>> GetOrRefreshAsync();
    void ClearCache();
}
