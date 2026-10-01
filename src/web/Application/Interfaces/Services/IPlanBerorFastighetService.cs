using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Services;

public interface IPlanBerorFastighetService
{
    Task<IEnumerable<PlanBerorFastighet>> GetAllAsync();
}
