namespace Plandokument.Application.Options.Application.Security;

public class AdminIdentities
{
    public string[] AllowedUsers { get; set; } = Array.Empty<string>();
    public string[] AllowedGroups { get; set; } = Array.Empty<string>();
}