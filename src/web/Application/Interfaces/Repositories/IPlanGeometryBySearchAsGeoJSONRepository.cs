using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Repositories;

public interface IPlanGeometryBySearchAsGeoJSONRepository
{
    Task<PlanGeoJSON> GetPlanGeometryBySearchAsGeoJSONAsync(List<string> planIds, CancellationToken cancellationToken = default);
}