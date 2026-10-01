using Plandokument.Domain.Entities;
using Plandokument.Domain.Search;

namespace Plandokument.Application.Interfaces.Services;

public interface IPlanBerorPlanService
{
    Task<IEnumerable<PlanBerorPlan>> GetAllAsync();
    Task<IEnumerable<PlanBerorPlan>> GetBySpecificPlansAsync(List<string> planIds);

}
