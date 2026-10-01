namespace Plandokument.Domain.Search;

public class PlanFileResult
{
    public string Path { get; set; }
    public string PathVirtual { get; set; }
    public string Name { get; set; }
    public string Extension { get; set; }
    public long Size { get; set; }
    public string PlanId { get; set; }
    public string DocumentType { get; set; }
    public string FindType { get; set; }
    public string DocumentPart { get; set; }
    public string ThumbnailPath { get; set; }
    public string ThumbnailIndication { get; set; }
}