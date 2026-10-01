using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Domain.Entities;

namespace Plandokument.Application.Services;

public class DocumentTypeService : IDocumentTypeService
{
    private readonly IDocumentTypeCacheService _cacheService;

    public DocumentTypeService(IDocumentTypeCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<List<DocumentType>> GetAllAsync()
    {
        return await _cacheService.GetOrRefreshAsync();
    }
}
