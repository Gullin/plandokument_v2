using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Caches;

public interface IPlanBasCacheService
{
    Task<IEnumerable<PlanBas>> GetOrRefreshAsync();
    Task<IEnumerable<PlanBas>> RefreshAsync();
    void ClearCache();
}
