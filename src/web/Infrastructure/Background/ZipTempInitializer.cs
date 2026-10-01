using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Plandokument.Infrastructure.Background;

public class ZipTempInitializer : IHostedService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ZipTempInitializer> _logger;

    public ZipTempInitializer(IWebHostEnvironment env, ILogger<ZipTempInitializer> logger)
    {
        _env = env;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(_env.ContentRootPath, "zipTemp");
        try
        {
            Directory.CreateDirectory(path);
            _logger.LogInformation("Ensured zipTemp directory exists at {Path}", path);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Could not create zipTemp directory at {Path}", path);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
