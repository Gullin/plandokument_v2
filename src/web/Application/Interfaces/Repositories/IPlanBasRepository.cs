using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Repositories;

public interface IPlanBasRepository
{
    Task<IEnumerable<PlanBas>> GetPlanBasAsync(CancellationToken cancellationToken = default);
}
