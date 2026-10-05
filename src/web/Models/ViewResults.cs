using Plandokument.Domain.Search;

namespace Plandokument.Models;

public class ViewResults
{
    public bool IsSearched{ get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public GeneralInfo GeneralInfo { get; set; } = new();
    public DocumentTypeInfo DocumentTypeInfo { get; set; } = new();
    public string[] SearchTerms { get; set; } = Array.Empty<string>();
    public string? SearchDocumentType { get; set; }
    public string? SearchType { get; set; }
    public List<PlanSearchResult> Plans { get; set; } = new();
}
