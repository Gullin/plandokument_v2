using Microsoft.Extensions.Options;
using Plandokument.Application.Interfaces.Repositories;
using Plandokument.Application.Options.PlanDocumentFiles;
using Plandokument.Domain.Entities;

namespace Plandokument.Infrastructure.Repositories;

public class PlanDocumentsRepository : IPlanDocumentsRepository
{
    private readonly PlanDocumentFiles _options;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<PlanDocumentsRepository> _logger;

    public PlanDocumentsRepository(IOptions<PlanDocumentFiles> options, IWebHostEnvironment webHostEnvironment, ILogger<PlanDocumentsRepository> logger)
    {
        _options = options.Value;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    async Task<IEnumerable<PlanDocuments>> IPlanDocumentsRepository.GetPlanDocumentsAsync(
        CancellationToken cancellationToken
        )
    {
        return await Task.Run(() =>
        {
            try
            {

                var rootPath = _options.Paths;
                var crawlSubDirectories = _options.CrawlSubDirectories;
                var fileTypes = _options.FileTypes;

                // Gör extensions lookup snabbare
                var extLookup = new HashSet<string>(
                    fileTypes.Select(e => e.StartsWith('.') ? e : "." + e),
                    StringComparer.OrdinalIgnoreCase);

                var results = new List<PlanDocuments>();

                foreach (var root in rootPath)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Hantera relativa sökvägar (till wwwroot eller ContentRoot)
                    var fullPath = Path.IsPathRooted(root.Physical)
                        ? root.Physical
                        : Path.Combine(_webHostEnvironment.ContentRootPath, root.Physical);

                    var dirInfo = new DirectoryInfo(fullPath);

                    if (!dirInfo.Exists)
                        continue;

                    var excludeDir = Path.Combine(dirInfo.FullName, root.ThumbnailSubFolder);

                    // Hämta filer med angivna extension, men exkludera thumbnails-katalogen
                    var files = dirInfo
                        .EnumerateFiles("*", SearchOption.AllDirectories)
                        .Where(f => extLookup.Contains(f.Extension))
                        .Where(f => !f.DirectoryName!.StartsWith(excludeDir, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    results.Add(
                        new PlanDocuments
                        {
                            RootPath = fullPath,
                            VirtualPath = root.Virtual,
                            Documents = files
                        });
                }

                return results.AsEnumerable();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing GetPlanDocumentsAsync");

                throw;
            }
        }, cancellationToken);
    }
}
