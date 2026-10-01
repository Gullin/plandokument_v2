using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Options.Caches;
using Plandokument.Domain.Entities;

namespace Plandokument.Infrastructure.Background;

public class CacheRefreshService : BackgroundService
{
    private readonly ILogger<CacheRefreshService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _targetTime;

    public CacheRefreshService(
        ILogger<CacheRefreshService> logger,
        IServiceProvider serviceProvider,
        IOptions<CachesSettings> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
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
            _logger.LogInformation("Nästa DocumentType-cache refresh sker {NextRun}", nextRun);

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

            // Kör själva refresh
            using var scope = _serviceProvider.CreateScope();
            var documentTypeCacheService = scope.ServiceProvider.GetRequiredService<IDocumentTypeCacheService>();
            var planBasCacheService = scope.ServiceProvider.GetRequiredService<IPlanBasCacheService>();
            var planBerorPlanCacheService = scope.ServiceProvider.GetRequiredService<IPlanBerorPlanCacheService>();
            var planBerorFastighetCacheService = scope.ServiceProvider.GetRequiredService<IPlanBerorFastighetCacheService>();
            var planGeometriesAllAsGeoJSONCacheService = scope.ServiceProvider.GetRequiredService<IPlanGeometriesAllAsGeoJSONCacheService>();
            var planDocumentsCacheService = scope.ServiceProvider.GetRequiredService<IPlanDocumentsCacheService>();

            try
            {
                var docs = await documentTypeCacheService.GetOrRefreshAsync();
                _logger.LogInformation("DocumentType-cachen uppdaterad ({Count} rader) {Time}",
                    docs.Count, DateTime.Now);

                var planBas = await planBasCacheService.GetOrRefreshAsync();
                _logger.LogInformation("PlanBas-cachen uppdaterad ({Count} rader) {Time}",
                    planBas is ICollection<PlanBas> coll ? coll.Count : planBas.Count(), DateTime.Now);

                var planBerorPlan = await planBerorPlanCacheService.GetOrRefreshAsync();
                _logger.LogInformation("PlanBerorPlan-cachen uppdaterad ({Count} rader) {Time}",
                    planBerorPlan.Count(), DateTime.Now);

                var planBerorFastighet = await planBerorFastighetCacheService.GetOrRefreshAsync();
                _logger.LogInformation("PlanBerorFastighet-cachen uppdaterad ({Count} rader) {Time}",
                    planBerorFastighet.Count(), DateTime.Now);

                var planGeometriesAllAsGeoJSON = await planGeometriesAllAsGeoJSONCacheService.GetOrRefreshAsync();
                _logger.LogInformation("PlanGeometriesAllAsGeoJSON-cachen uppdaterad ({Count} rader) {Time}",
                    planGeometriesAllAsGeoJSON.Count(), DateTime.Now);

                var planDocuments = await planDocumentsCacheService.GetOrRefreshAsync();
                _logger.LogInformation("planDocuments-cachen uppdaterad ({Count} rader) {Time}",
                    planDocuments.Count(), DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid refresh av cache");
            }
        }
    }
}
