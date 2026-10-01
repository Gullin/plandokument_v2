namespace Plandokument.Domain.Entities;

public class CompressDocumentsRequest
{
    public List<string> DocumentPaths { get; set; } = new();
    public string BaseName { get; set; } = string.Empty;
}
