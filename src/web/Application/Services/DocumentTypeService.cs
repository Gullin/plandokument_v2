using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Domain.Entities;
using System.ComponentModel;
using System.Reflection;

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

    public int GetPropertyCount()
    {
        return typeof(DocumentType).GetProperties().Length;
    }

    public Dictionary<string, string?> GetPropertyDescriptions()
    {
        return typeof(DocumentType)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(
                p => p.Name,
                p => p.GetCustomAttribute<DescriptionAttribute>()?.Description);
    }

    public async Task<int> GetDocumentTypeCountAsync()
    {
        var documentTypes = await GetAllAsync();
        return documentTypes.Count;
    }
}
