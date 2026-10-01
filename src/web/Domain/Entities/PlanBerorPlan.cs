namespace Plandokument.Domain.Entities;

public class PlanBerorPlan
{
    public required string nyckel{ get; set; }
    public required string planfk { get; set; }
    public required string beskrivning { get; set; }
    public int? nyckel_pavarkan { get; set; }
    public required string? pavarkan { get; set; }
    public required string? pav_planfk { get; set; }
    public required string? status_pavarkan { get; set; }
    public int registrerat_beslut { get; set; }
}
