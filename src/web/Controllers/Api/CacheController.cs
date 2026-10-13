using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Interfaces.Exporters;

namespace Plandokument.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class CacheController : ControllerBase
{
    private readonly ICacheRegistry _cacheRegistry;
    private readonly ICacheCsvExporter _exporter;

    public CacheController(ICacheRegistry cacheRegistry, ICacheCsvExporter exporter)
    {
        _cacheRegistry = cacheRegistry;
        _exporter = exporter;
    }

    [HttpGet("caches")]
    public IActionResult AllCaches()
    {
        var caches = _cacheRegistry
            .GetAll()
            .Select(x => new
            {
                name = x.Name,
                cacheKey = x.CacheKey
            });

        return Ok(caches);
    }

    [HttpGet("{cacheKey}/export")]
    public async Task<IActionResult> Export(
        string cacheKey,
        CancellationToken cancellationToken)
    {
        // Validera att cacheKey finns.
        _cacheRegistry.Get(cacheKey);

        var path = await _exporter.ExportToFileAsync(
            cacheKey,
            cancellationToken);

        return Ok(new
        {
            cache = cacheKey,
            file = path
        });
    }

    [HttpGet("{cacheKey}/export/stream")]
    public async Task ExportStream(
    string cacheKey,
    CancellationToken cancellationToken)
    {
        // Validera att cacheKey finns.
        _cacheRegistry.Get(cacheKey);

        Response.ContentType = "text/csv; charset=utf-8";

        Response.Headers.ContentDisposition =
            $"attachment; filename=\"{cacheKey}.csv\"";

        await _exporter.ExportToStreamAsync(
            cacheKey,
            Response.Body,
            cancellationToken);
    }

    [HttpPost("{cacheKey}/refresh")]
    public async Task<IActionResult> Refresh(
    string cacheKey,
    CancellationToken cancellationToken)
    {
        var cache = _cacheRegistry.Get(cacheKey);

        var data = await cache.RefreshAsync(cancellationToken);

        return Ok(new
        {
            cacheKey,
            refreshed = true
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAll(
    CancellationToken cancellationToken)
    {
        var results = new List<object>();

        foreach (var cache in _cacheRegistry.GetAll())
        {
            var data = await cache.RefreshAsync(cancellationToken);

            results.Add(new
            {
                cache.Name,
                cache.CacheKey,
                refreshed = true
            });
        }

        return Ok(results);
    }
}