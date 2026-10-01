using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Domain.Entities;

namespace Plandokument.Application.Services;

public class PlanBasService : IPlanBasService
{
    private readonly IPlanBasCacheService _cacheService;

    public PlanBasService(IPlanBasCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<PlanBas>> GetAllAsync()
    {
        return await _cacheService.GetOrRefreshAsync();
    }
}
