using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Interfaces.Repositories;
using Plandokument.Application.Options.Caches;
using Plandokument.Domain.Entities;

namespace Plandokument.Infrastructure.Cache;

public class PlanBerorFastighetCacheService : IPlanBerorFastighetCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IPlanBerorFastighetRepository _repository;
    private readonly CachesSettings _cacheSettings;
    private readonly PlanBerorFastighetCacheOptions _options;

    public PlanBerorFastighetCacheService(
        IMemoryCache cache,
        IPlanBerorFastighetRepository repository,
        IOptions<CachesSettings> options)
    {
        _cache = cache;
        _repository = repository;
        _cacheSettings = options.Value;
        _options = options.Value.PlanBerorFastighet;
    }

    public async Task<IEnumerable<PlanBerorFastighet>> GetOrRefreshAsync()
    {
        if (_cache.TryGetValue(
            _options.CacheKey,
            out IEnumerable<PlanBerorFastighet>? cached))
        {
            return cached;
        }

        return await RefreshAsync();
    }

    public async Task<IEnumerable<PlanBerorFastighet>> RefreshAsync()
    {
        var repo = await _repository.GetPlanBerorFastighetAsync();

        var now = DateTime.Now;
        var nextRefresh = now.Date.Add(_cacheSettings.RefreshTime);

        if (nextRefresh <= now)
        {
            nextRefresh = nextRefresh.AddDays(1);
        }

        _cache.Set(_options.CacheKey, repo, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = nextRefresh
        });

        return repo;
    }

    public void ClearCache()
    {
        _cache.Remove(_options.CacheKey);
    }
}
