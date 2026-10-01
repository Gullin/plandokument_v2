using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Caches
{
    public interface IDocumentTypeCacheService
    {
        Task<List<DocumentType>> GetOrRefreshAsync();
        void ClearCache();
    }
}
