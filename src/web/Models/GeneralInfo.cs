using Plandokument.Application.Options.Application;

namespace Plandokument.Models;

public class GeneralInfo
{
    public string VersionInfo { get; set; } = string.Empty;
    public string Copyright { get; set; } = string.Empty;
    public Search SearchParams { get; set; } = new();
}
