using Microsoft.Extensions.Options;
using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Interfaces.Exporters;
using Plandokument.Application.Options.Logging;
using System.Text;

namespace Plandokument.Application.Services;

public sealed class CacheCsvExporter : ICacheCsvExporter
{
    private readonly IWebHostEnvironment _environment;
    private readonly IOptions<ApplicationSpecificLoggingSettings> _loggingOptions;
    private readonly ICacheRegistry _cacheRegistry;
    private readonly ILogger<CacheCsvExporter> _logger;

    public CacheCsvExporter(
        IWebHostEnvironment environment,
        IOptions<ApplicationSpecificLoggingSettings> loggingOptions,
        ICacheRegistry cacheRegistry,
        ILogger<CacheCsvExporter> logger)
    {
        _environment = environment;
        _loggingOptions = loggingOptions;
        _cacheRegistry = cacheRegistry;
        _logger = logger;
    }

    public Task<string> ExportToFileAsync(
        string cacheKey,
        CancellationToken cancellationToken = default)
    {
        var configuredRootPath = _loggingOptions.Value.RootPath;

        if (string.IsNullOrWhiteSpace(configuredRootPath))
        {
            throw new InvalidOperationException(
                "Logging:ApplicationSpecificLogging:RootPath måste vara konfigurerad.");
        }

        var logDirectory = Path.IsPathFullyQualified(configuredRootPath)
            ? configuredRootPath
            : Path.GetFullPath(
                Path.Combine(
                    _environment.ContentRootPath,
                    configuredRootPath));

        var fileName =
            $"{cacheKey}{DateTime.Now:yyyyMMddTHHmmss}.csv";

        var path = Path.Combine(logDirectory, fileName);

        return ExportToFileAsync(
            cacheKey,
            path,
            cancellationToken);
    }

    public async Task<string> ExportToFileAsync(
        string cacheKey,
        string path,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.IsPathFullyQualified(path)
            ? path
            : Path.GetFullPath(
                Path.Combine(
                    _environment.ContentRootPath,
                    path));

        var directory = Path.GetDirectoryName(fullPath);

        if (string.IsNullOrEmpty(directory))
        {
            throw new InvalidOperationException(
                $"Kunde inte bestämma katalog för '{fullPath}'.");
        }

        Directory.CreateDirectory(directory);

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read,
            bufferSize: 64 * 1024,
            useAsync: true);

        await ExportToStreamAsync(
            cacheKey,
            fileStream,
            cancellationToken);

        _logger.LogInformation(
            "CSV-export av cache {CacheName} skapad: {FilePath}",
            cacheKey,
            fullPath);

        return fullPath;
    }

    public async Task ExportToStreamAsync(
        string cacheKey,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        var descriptor = _cacheRegistry.Get(cacheKey);

        var data = await descriptor.GetAsync(cancellationToken);

        await using var writer = new StreamWriter(
            destination,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            bufferSize: 64 * 1024,
            leaveOpen: true);

        await descriptor.CsvWriter.WriteAsync(
            data,
            writer,
            cancellationToken);

        await writer.FlushAsync(cancellationToken);
    }
}