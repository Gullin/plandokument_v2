using Plandokument.Infrastructure.Cache;

namespace Plandokument.Application.Interfaces.Caches;

public interface ICacheRegistry
{
    IEnumerable<CacheDescriptor> GetAll();

    CacheDescriptor Get(string cacheKey);
}