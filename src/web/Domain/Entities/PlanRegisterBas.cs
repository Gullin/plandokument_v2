namespace Plandokument.Domain.Entities;

public class PlanRegisterBas
{
    public required string plan_id { get; set; }
    public string? lmakt { get; set; }
    public string? egn_akt { get; set; }
    public required string uuid { get; set; }
    public required string planfk { get; set; }
    public string? plannamn { get; set; }
    public required string status { get; set; }
    public required string status_text { get; set; }
    public int isgenomf { get; set; }
    public string? dat_beslut { get; set; }
    public string? dat_genomf_f { get; set; }
    public string? dat_genomf_t { get; set; }
    public string? komdkod { get; set; }
}
