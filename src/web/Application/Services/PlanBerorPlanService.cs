using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Domain.Entities;
using Plandokument.Domain.Search;

namespace Plandokument.Application.Services;

public class PlanBerorPlanService : IPlanBerorPlanService
{
    private readonly IPlanBerorPlanCacheService _cacheService;

    public PlanBerorPlanService(IPlanBerorPlanCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<PlanBerorPlan>> GetAllAsync()
    {
        return await _cacheService.GetOrRefreshAsync();
    }

    public async Task<IEnumerable<PlanBerorPlan>> GetBySpecificPlansAsync(List<string> planIds)
    {
        var cachedPlansBerorPlans = await _cacheService.GetOrRefreshAsync();
        return cachedPlansBerorPlans.Where(p => planIds.Contains(p.nyckel)).ToList();
    }
}
