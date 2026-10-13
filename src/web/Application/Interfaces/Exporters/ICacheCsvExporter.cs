namespace Plandokument.Application.Interfaces.Exporters;

public interface ICacheCsvExporter
{
    Task<string> ExportToFileAsync(
        string cacheName,
        CancellationToken cancellationToken = default);

    Task<string> ExportToFileAsync(
        string cacheName,
        string path,
        CancellationToken cancellationToken = default);

    Task ExportToStreamAsync(
        string cacheName,
        Stream destination,
        CancellationToken cancellationToken = default);
}
