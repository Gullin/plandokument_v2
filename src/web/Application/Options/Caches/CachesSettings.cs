namespace Plandokument.Application.Options.Caches;

public class CachesSettings
{
    /// <summary>
    /// Global tid på dygnet då alla cacher ska förnyas (HH:mm:ss).
    /// </summary>
    public TimeSpan RefreshTime { get; set; }

    public DocumentTypesCacheOptions DocumentTypes { get; set; } = new();
    public PlanBasCacheOptions PlanBas { get; set; } = new();
    public PlanBerorPlanCacheOptions PlanBerorPlan { get; set; } = new();
    public PlanBerorFastighetCacheOptions PlanBerorFastighet { get; set; } = new();
    public PlanGeometriesAllAsGeoJSONCacheOptions PlanGeometriesAllAsGeoJSON { get; set; } = new();
    public PlanDocumentsCacheOptions PlanDocuments { get; set; } = new();
}
