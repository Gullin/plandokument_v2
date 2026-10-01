using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Plandokument.Application.Options.PlanDocumentFiles;
using Plandokument.Domain.Entities;
using System.IO.Compression;

namespace Plandokument.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class CompressionController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly PlanDocumentFiles _docFileConfig;


    public CompressionController(
        IWebHostEnvironment env,
        IOptions<PlanDocumentFiles> docFileConfig)
    {
        _env = env;
        _docFileConfig = docFileConfig.Value;
    }

    [HttpPost("zip/create")]
    public async Task<IActionResult> CreateZip(CompressDocumentsRequest request)
    {
        if (request.DocumentPaths == null || request.DocumentPaths.Count == 0)
            return BadRequest("Ingen dokumentlista angiven.");

        if (string.IsNullOrWhiteSpace(request.BaseName))
            return BadRequest("Zip-filens namn saknas.");

        var zipSubFolder = "zipTemp";
        var zipRoot = Path.Combine(_env.ContentRootPath, zipSubFolder);
        Directory.CreateDirectory(zipRoot);
        var httpRequest = HttpContext.Request;
        var zipBaseUrl = $"{httpRequest.Scheme}://{httpRequest.Host}/{zipSubFolder}/";

        await CleanupOldZipsAsync(zipRoot);

        var timestamp = DateTime.Now.ToString("yyyyMMddTHHmmss.fff");
        var zipFileName = $"{request.BaseName.Replace("/", "_")}_{timestamp}.zip";
        var zipPath = Path.Combine(zipRoot, zipFileName);
        var zipVirtualUrl = $"{zipBaseUrl}{zipFileName}";

        // Skapa zip-filen
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (var virtualPath in request.DocumentPaths)
            {
                var physicalPath = ResolvePhysicalPath(virtualPath);

                if (physicalPath == null || !System.IO.File.Exists(physicalPath))
                    continue;

                zip.CreateEntryFromFile(
                    physicalPath,
                    Path.GetFileName(physicalPath),
                    CompressionLevel.Fastest);
            }
        } // ← HÄR stängs zip-filen och filhandtaget släpps

        // Alternativ som URL till nedladdning, kräver att zip-mappen är tillgänglig via webben
        return Ok(new
        {
            ZipFileName = zipFileName,
            ZipPath = zipVirtualUrl
        });

        // Alternativ som filström (inte lämplig för stora filer)
        //var fileStream = new FileStream(
        //    zipPath,
        //    FileMode.Open,
        //    FileAccess.Read,
        //    FileShare.Read);

        //return File(
        //    fileStream,
        //    "application/zip",
        //    zipFileName);
    }

    /// <summary>
    /// Raderar alla zip-filer äldre än 12 timmar.
    /// </summary>
    /// <param name="zipRoot">Sökväg till zip-katalog som innehåller alla tidigare skapade zip-filer</param>
    /// <returns></returns>
    private static async Task CleanupOldZipsAsync(string zipRoot)
    {
        await Task.Run(() =>
        {
            var expiration = DateTime.Now.AddHours(-12);

            var files = Directory.GetFiles(zipRoot, "*.zip");

            foreach (var file in files)
            {
                var created = System.IO.File.GetCreationTime(file);
                if (created < expiration)
                {
                    System.IO.File.Delete(file);
                }
            }
        });
    }

    /// <summary>
    /// Hjälpmetod som konverterar virtuell sökväg till fysisk sökväg genom att första segmentet i den virtuella sökvägen utgör nyckeln
    /// </summary>
    /// <param name="virtualPath">Sökväg till dokument virtuellt</param>
    /// <returns></returns>
    private string? ResolvePhysicalPath(string virtualPath)
    {
        // Normalisera
        var normalized = virtualPath
            .Replace('\\', '/')
            .TrimStart('/');

        var firstSegment = normalized.Split('/', 2)[0];

        var mapping = _docFileConfig.Paths
            .FirstOrDefault(p =>
                string.Equals(p.Virtual, firstSegment, StringComparison.OrdinalIgnoreCase));

        if (mapping == null)
            return null;

        var relativePart = normalized.Substring(firstSegment.Length).TrimStart('/');

        return Path.Combine(mapping.Physical, relativePart);
    }

}
