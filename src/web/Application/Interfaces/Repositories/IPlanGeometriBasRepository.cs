using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Repositories;

public interface IPlanGeometriBasRepository
{
    Task<IEnumerable<PlanGeometriBas>> GetPlanGeometriBasAsync(CancellationToken cancellationToken = default);
}
