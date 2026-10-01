using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Domain.Entities;

namespace Plandokument.Application.Services;

public class PlanBerorFastighetService : IPlanBerorFastighetService
{
    private readonly IPlanBerorFastighetCacheService _cacheService;

    public PlanBerorFastighetService(IPlanBerorFastighetCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<PlanBerorFastighet>> GetAllAsync()
    {
        return await _cacheService.GetOrRefreshAsync();
    }
}
