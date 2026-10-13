using Plandokument.Application.Interfaces.Exporters;

namespace Plandokument.Infrastructure.Cache;

public abstract class CacheCsvWriter<T> : ICacheCsvWriter
{
    public Task WriteAsync(
        object data,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        if (data is not IEnumerable<T> typedData)
        {
            throw new ArgumentException(
                $"Förväntade IEnumerable<{typeof(T).Name}>.",
                nameof(data));
        }

        return WriteTypedAsync(
            typedData,
            writer,
            cancellationToken);
    }

    protected abstract Task WriteTypedAsync(
        IEnumerable<T> data,
        TextWriter writer,
        CancellationToken cancellationToken);
}