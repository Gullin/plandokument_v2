using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Caches;

public interface IPlanBerorFastighetCacheService
{
    Task<IEnumerable<PlanBerorFastighet>> GetOrRefreshAsync();
    void ClearCache();
}
