using System.ComponentModel;

namespace Plandokument.Domain.Entities;

public class DocumentType
{
    [Description("Dokumenttyp")]
    public string Type { get; set; }
    [Description("Filter för användning i URL för begränsning av dokumenttyper")]
    public string UrlFilter { get; set; }
    [Description("Ändelse i dokumentets filnamn som indikerar dokumenttypen")]
    public string Suffix { get; set; }
    [Description("Beskrivning som valfri förklarande text")]
    public string Description { get; set; }
    [Description("Signalerar om dokumenttypen är en planhandling eller inte. För värden som inte är en dokumenttyp ges värde false.")]
    public bool IsPlanhandling { get; set; }
}
