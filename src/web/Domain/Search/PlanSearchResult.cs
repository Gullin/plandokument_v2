namespace Plandokument.Domain.Search;

public class PlanSearchResult
{
    public string Nyckel { get; set; }
    public string Akt { get; set; }
    public string? AktEgen { get; set; }
    public string? AktPb { get; set; }
    public string PlanFk { get; set; }
    public string? PlanNamn { get; set; }
    public bool IsGenomf { get; set; }
    public string? Begrepp { get; set; }
    public string SearchedString { get; set; }
}
