using Plandokument.Application.Options.PlanDocumentFiles;
using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Caches;

public interface IPlanDocumentsCacheService
{
    Task<IEnumerable<PlanDocuments>> GetOrRefreshAsync();
    void ClearCache();
}
