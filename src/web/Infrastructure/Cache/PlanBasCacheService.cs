using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Interfaces.Repositories;
using Plandokument.Application.Options.Caches;
using Plandokument.Domain.Entities;

namespace Plandokument.Infrastructure.Cache;

public class PlanBasCacheService : IPlanBasCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IPlanBasRepository _repository;
    private readonly CachesSettings _cacheSettings;
    private readonly PlanBasCacheOptions _options;

    public PlanBasCacheService(
        IMemoryCache cache,
        IPlanBasRepository repository,
        IOptions<CachesSettings> options)
    {
        _cache = cache;
        _repository = repository;
        _cacheSettings = options.Value;
        _options = options.Value.PlanBas;
    }

    public async Task<IEnumerable<PlanBas>> GetOrRefreshAsync()
    {
        if (_cache.TryGetValue(_options.CacheKey, out List<PlanBas> cached))
        {
            return cached;
        }

        var planBas = await _repository.GetPlanBasAsync();

        var now = DateTime.Now;
        var nextRefresh = now.Date.Add(_cacheSettings.RefreshTime);
        if (nextRefresh <= now)
        {
            nextRefresh = nextRefresh.AddDays(1);
        }

        _cache.Set(_options.CacheKey, planBas, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = nextRefresh
        });

        return planBas;
    }

    public void ClearCache()
    {
        _cache.Remove(_options.CacheKey);
    }
}
