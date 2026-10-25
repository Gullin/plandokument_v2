using Plandokument.Application.Options.Application;

namespace Plandokument.Models;

public class GeneralInfo
{
    public string Version { get; set; } = string.Empty;
    public string VersionSuffix { get; set; } = string.Empty;
    public string VersionInfo { get; set; } = string.Empty;
    public string Copyright { get; set; } = string.Empty;
    public string GitCommitSha { get; init; } = string.Empty;
    public string GitCommitShaShort { get; init; } = string.Empty;
    public Search SearchParams { get; set; } = new();
}
