using Plandokument.Application.Options.Application.Security;
using System.Net;

namespace Plandokument.Application.Options.Application;

public class ApplicationSettings
{
    public string BasePath { get; set; }
    public string Version { get; set; } = string.Empty;
    public string VersionSuffix { get; set; } = string.Empty;
    public Security.AuthorizationSettings Authorizations { get; set; } = new();
    public Search SearchParams { get; set; } = new();
}
