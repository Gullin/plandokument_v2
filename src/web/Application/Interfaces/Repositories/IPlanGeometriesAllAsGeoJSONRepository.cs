using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Repositories;

public interface IPlanGeometriesAllAsGeoJSONRepository
{
    Task<IEnumerable<PlanGeoJSON>> GetPlanGeometriesAllAsGeoJSONAsync(CancellationToken cancellationToken = default);
}