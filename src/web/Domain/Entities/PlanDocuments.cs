namespace Plandokument.Domain.Entities;

public class PlanDocuments
{
    public string? RootPath { get; set; }
    public string? VirtualPath { get; set; }
    public List<FileInfo>? Documents { get; set; }
}
