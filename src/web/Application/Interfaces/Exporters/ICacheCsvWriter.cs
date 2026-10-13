namespace Plandokument.Application.Interfaces.Exporters;

public interface ICacheCsvWriter
{
    Task WriteAsync(
        object data,
        TextWriter writer,
        CancellationToken cancellationToken = default);
}