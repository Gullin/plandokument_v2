using Plandokument.Domain.Entities;

namespace Plandokument.Application.Interfaces.Services;

public interface IPlanBasService
{
    Task<IEnumerable<PlanBas>> GetAllAsync();
}
