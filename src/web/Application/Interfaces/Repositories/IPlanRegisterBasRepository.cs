using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Repositories;

public interface IPlanRegisterBasRepository
{
    Task<IEnumerable<PlanRegisterBas>> GetPlanRegisterBasAsync(CancellationToken cancellationToken = default);
}