using Microsoft.AspNetCore.Authorization;
using Plandokument.Application.Options.Application.Security;

namespace Plandokument.Infrastructure.Security;

public class AdOrUserAdminsHandler : AuthorizationHandler<AdOrUserAdminsRequirement>
{
    private readonly AuthorizationSettings _authOptions;
    private readonly ILogger<AdOrUserAdminsHandler> _logger;

    public AdOrUserAdminsHandler(
        Plandokument.Application.Options.Application.Security.AuthorizationSettings authOptions,
        ILogger<AdOrUserAdminsHandler> logger
        )
    {
        _authOptions = authOptions;
        _logger = logger;
    }

    /// <summary>
    /// Implementerar en Windows token-baserad användare- och rollkontroll. Finns begränsningar i hur många roller claims kan innehålla vilket kan medföra att en roll ev. inte hittas. För att undvika detta behöver IsInRole gås ifrån och uppslag istället göras direkt mot Active Directory genom LDAP.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="requirement"></param>
    /// <returns></returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdOrUserAdminsRequirement requirement
        )
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        var allowedUsers = _authOptions.Admins.AllowedUsers ?? Array.Empty<string>();

        var allowedGroups = _authOptions.Admins.AllowedGroups ?? Array.Empty<string>();

        var userName = context.User.Identity?.Name;

        _logger.LogDebug(
            "Auktoriserar användare '{UserName}' mot krav '{RequirementName}' med tillåtna användare: {AllowedUsers} och tillåtna grupper: {AllowedGroups}",
            userName,
            requirement.GetType().Name,
            string.Join(", ", allowedUsers),
            string.Join(", ", allowedGroups)
        );

        // 1. Kontrollera explicit användare
        if (allowedUsers.Contains(userName, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            _logger.LogDebug(
                "Användare '{UserName}' auktoriserad genom explicit användarlista",
                userName
            );
            return Task.CompletedTask;
        }

        // 2. Kontrollera AD-grupp
        foreach (var group in allowedGroups)
        {
            try
            {
                if (context.User.IsInRole(group))
                {
                    context.Succeed(requirement);
                    _logger.LogDebug(
                        "Användare '{UserName}' auktoriserad genom grupp '{Group}'",
                        userName,
                        group
                    );
                    return Task.CompletedTask;
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    $"Kunde inte slå upp Windows/AD-gruppen '{group}' vid auktorisering av användaren '{userName}'.");
            }
        }

        return Task.CompletedTask;
    }
}
