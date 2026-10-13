using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Caches;

public interface IDocumentTypeCacheService
{
    Task<IEnumerable<DocumentType>> GetOrRefreshAsync();
    Task<IEnumerable<DocumentType>> RefreshAsync();
    void ClearCache();
}
