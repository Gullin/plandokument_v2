namespace Plandokument.Application.Options.PlanDocumentFiles;

public class PlanDocumentFiles
{
    public StaticPath[]? Paths { get; set; }
    public bool CrawlSubDirectories { get; set; } = false;
    public string[]? FileTypes { get; set; }
}
