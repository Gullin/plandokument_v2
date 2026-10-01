using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Options.Caches;
using Plandokument.Domain.Entities;
using Plandokument.Domain.Interfaces;

namespace Plandokument.Infrastructure.Cache;

public class DocumentTypeCacheService : IDocumentTypeCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IDocumentTypeRepository _repository;
    private readonly CachesSettings _cacheSettings;
    private readonly DocumentTypesCacheOptions _options;

    public DocumentTypeCacheService(
        IMemoryCache cache,
        IDocumentTypeRepository repository,
        IOptions<CachesSettings> options)
    {
        _cache = cache;
        _repository = repository;
        _cacheSettings = options.Value;
        _options = options.Value.DocumentTypes;
    }

    public async Task<List<DocumentType>> GetOrRefreshAsync()
    {
        if (_cache.TryGetValue(_options.CacheKey, out List<DocumentType> cached))
        {
            return cached;
        }

        var documentTypes = await _repository.GetDocumentTypesAsync();

        var now = DateTime.Now;
        var nextRefresh = now.Date.Add(_cacheSettings.RefreshTime);
        if (nextRefresh <= now)
        {
            nextRefresh = nextRefresh.AddDays(1);
        }

        _cache.Set(_options.CacheKey, documentTypes, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = nextRefresh
        });

        return documentTypes;
    }

    public void ClearCache()
    {
        _cache.Remove(_options.CacheKey);
    }
}
