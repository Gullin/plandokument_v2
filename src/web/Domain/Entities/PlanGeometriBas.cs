namespace Plandokument.Domain.Entities;

public class PlanGeometriBas
{
    public required string lmakt { get; set; }
    public required string local_id_list { get; set; }
    public required string rk_gmlid_list { get; set; }
    public string? planavgift { get; set; }
    public string? akt_pb{ get; set; }
}
