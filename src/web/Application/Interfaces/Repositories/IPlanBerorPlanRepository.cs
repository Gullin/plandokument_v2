using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Repositories;

public interface IPlanBerorPlanRepository
{
    Task<IEnumerable<PlanBerorPlan>> GetPlanBerorPlanAsync(CancellationToken cancellationToken = default);
}