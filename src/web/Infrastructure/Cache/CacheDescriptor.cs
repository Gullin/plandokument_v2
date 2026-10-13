using Plandokument.Application.Interfaces.Exporters;

namespace Plandokument.Infrastructure.Cache;

public sealed class CacheDescriptor
{
    public required string CacheKey { get; init; }

    public required string Name { get; init; }

    public required Func<CancellationToken, Task<object>> GetAsync { get; init; }

    public required Func<CancellationToken, Task<object>> RefreshAsync { get; init; }

    public required ICacheCsvWriter CsvWriter { get; init; }
}