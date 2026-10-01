using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Repositories;

public interface IPlanBerorFastighetRepository
{
    Task<IEnumerable<PlanBerorFastighet>> GetPlanBerorFastighetAsync(CancellationToken cancellationToken = default);
}