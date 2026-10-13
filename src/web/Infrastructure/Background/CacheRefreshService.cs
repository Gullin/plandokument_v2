using Microsoft.Extensions.Options;
using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Options.Caches;
using Plandokument.Domain.Entities;
using Plandokument.Infrastructure.Cache;

namespace Plandokument.Infrastructure.Background;

public class CacheRefreshService : BackgroundService
{
    private readonly ILogger<CacheRefreshService> _logger;
    private readonly ICacheRegistry _cacheRegistry;
    private readonly TimeSpan _targetTime;

    public CacheRefreshService(
        ILogger<CacheRefreshService> logger,
        ICacheRegistry cacheRegistry,
        IOptions<CachesSettings> options)
    {
        _logger = logger;
        _cacheRegistry = cacheRegistry;
        _targetTime = options.Value.RefreshTime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = now.Date.Add(_targetTime);

            if (nextRun <= now)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            _logger.LogInformation("Nästa cache refresh sker {NextRun}", nextRun);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("CacheRefreshService, bakgrundsservice cancelled");
                break; // avbryts vid shutdown
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("CacheRefreshService, bakgrundsservice cancelled");
                break; // avbryts vid shutdown
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "Unhandled exception in background service {Service}",
                    GetType().Name
                );

                throw;
            }

            foreach (var cache in _cacheRegistry.GetAll())
            {
                try
                {
                    var data = await cache.RefreshAsync(stoppingToken);

                    _logger.LogInformation(
                        "Cache {CacheKey} ({Name}) uppdaterad {Time}.",
                        cache.CacheKey,
                        cache.Name,
                        DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Fel vid refresh av cache {CacheKey}.",
                        cache.CacheKey);
                }
            }
        }
    }
}
