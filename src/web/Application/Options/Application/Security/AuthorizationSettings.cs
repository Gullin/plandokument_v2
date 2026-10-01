namespace Plandokument.Application.Options.Application.Security;

public class AuthorizationSettings
{
    public AdminIdentities Admins { get; set; } = new();
}