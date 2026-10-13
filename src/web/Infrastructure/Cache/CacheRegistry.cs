using Microsoft.Extensions.Options;
using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Options.Caches;
using Plandokument.Infrastructure.Export.Csv;

namespace Plandokument.Infrastructure.Cache;

public sealed class CacheRegistry : ICacheRegistry
{
    private readonly IReadOnlyDictionary<string, CacheDescriptor> _caches;

    public CacheRegistry(
        IOptions<CachesSettings> options,
        IDocumentTypeCacheService documentTypeCacheService,
        IPlanBasCacheService planBasCacheService,
        IPlanBerorPlanCacheService planBerorPlanCacheService,
        IPlanBerorFastighetCacheService planBerorFastighetCacheService,
        IPlanGeometriesAllAsGeoJSONCacheService planGeometriesAllAsGeoJSONCacheService,
        IPlanDocumentsCacheService planDocumentsCacheService,
        DocumentTypeCsvWriter documentTypeCsvWriter,
        PlanBasCsvWriter planBasCsvWriter,
        PlanBerorPlanCsvWriter planBerorPlanCsvWriter,
        PlanBerorFastighetCsvWriter planBerorFastighetCsvWriter,
        PlanGeometriesAllAsGeoJSONCsvWriter planGeometriesAllAsGeoJSONCsvWriter,
        PlanDocumentsCsvWriter planDocumentsCsvWriter
        )
    {
        var settings = options.Value;

        _caches = new Dictionary<string, CacheDescriptor>(
            StringComparer.OrdinalIgnoreCase)
        {
            [settings.DocumentTypes.CacheKey] = new CacheDescriptor
            {
                CacheKey = settings.DocumentTypes.CacheKey,
                Name = nameof(settings.DocumentTypes),
                GetAsync = async cancellationToken =>
                    await documentTypeCacheService.GetOrRefreshAsync(),

                RefreshAsync = async cancellationToken =>
                    await documentTypeCacheService.RefreshAsync(),

                CsvWriter = documentTypeCsvWriter
            },

            [settings.PlanBas.CacheKey] = new CacheDescriptor
            {
                CacheKey = settings.PlanBas.CacheKey,
                Name = nameof(settings.PlanBas),
                GetAsync = async cancellationToken =>
                    await planBasCacheService.GetOrRefreshAsync(),

                RefreshAsync = async cancellationToken =>
                    await planBasCacheService.RefreshAsync(),

                CsvWriter = planBasCsvWriter
            },

            [settings.PlanBerorPlan.CacheKey] = new CacheDescriptor
            {
                CacheKey = settings.PlanBerorPlan.CacheKey,
                Name = nameof(settings.PlanBerorPlan),
                GetAsync = async cancellationToken =>
                    await planBerorPlanCacheService.GetOrRefreshAsync(),

                RefreshAsync = async cancellationToken =>
                    await planBerorPlanCacheService.RefreshAsync(),

                CsvWriter = planBerorPlanCsvWriter
            },

            [settings.PlanBerorFastighet.CacheKey] = new CacheDescriptor
            {
                CacheKey = settings.PlanBerorFastighet.CacheKey,
                Name = nameof(settings.PlanBerorFastighet),
                GetAsync = async cancellationToken =>
                    await planBerorFastighetCacheService.GetOrRefreshAsync(),

                RefreshAsync = async cancellationToken =>
                    await planBerorFastighetCacheService.RefreshAsync(),

                CsvWriter = planBerorFastighetCsvWriter
            },

            [settings.PlanGeometriesAllAsGeoJSON.CacheKey] = new CacheDescriptor
            {
                CacheKey = settings.PlanGeometriesAllAsGeoJSON.CacheKey,
                Name = nameof(settings.PlanGeometriesAllAsGeoJSON),
                GetAsync = async cancellationToken =>
                    await planGeometriesAllAsGeoJSONCacheService.GetOrRefreshAsync(),

                RefreshAsync = async cancellationToken =>
                    await planGeometriesAllAsGeoJSONCacheService.RefreshAsync(),

                CsvWriter = planGeometriesAllAsGeoJSONCsvWriter
            },

            [settings.PlanDocuments.CacheKey] = new CacheDescriptor
            {
                CacheKey = settings.PlanDocuments.CacheKey,
                Name = nameof(settings.PlanDocuments),
                GetAsync = async cancellationToken =>
                    await planDocumentsCacheService.GetOrRefreshAsync(),

                RefreshAsync = async cancellationToken =>
                    await planDocumentsCacheService.RefreshAsync(),

                CsvWriter = planDocumentsCsvWriter
            }
        };
    }

    public IEnumerable<CacheDescriptor> GetAll()
    {
        return _caches.Values.ToList().AsReadOnly();
    }

    public CacheDescriptor Get(string cacheKey)
    {
        if (!_caches.TryGetValue(cacheKey, out var cache))
        {
            throw new KeyNotFoundException(
                $"Ingen cache finns med CacheKey '{cacheKey}'.");
        }

        return cache;
    }
}