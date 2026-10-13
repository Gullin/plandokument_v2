using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Caches;

public interface IPlanDocumentsCacheService
{
    Task<IEnumerable<PlanDocuments>> GetOrRefreshAsync();
    Task<IEnumerable<PlanDocuments>> RefreshAsync();
    void ClearCache();
}
