using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Features.FeatureManagement.Entities;
using Plandokument.Features.FeatureManagement.Features;

namespace Plandokument.Features.FeatureManagement.Filters;

/// <summary>
/// Combines the application-wide feature state with the controller/action's
/// explicit request to show the sidebar.
/// </summary>
public sealed class FeatureFilter(IFeatureService featureService) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var enabled = featureService.IsEnabled(
            context.HttpContext,
            FeatureNames.AdminMeny);

        context.HttpContext.Items[nameof(Plandokument.Domain.Interfaces.IFeatureState)] =
            new FeatureState { AdminMenyEnabled = enabled };
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Controller is not Controller controller)
            return;

        var state = context.HttpContext.Items[nameof(Plandokument.Domain.Interfaces.IFeatureState)]
            as FeatureState;

        // Both conditions are required:
        // 1. The feature is enabled/released.
        // 2. The controller/action explicitly requests the sidebar.
        var explicitlyRequested = controller.ViewData["ShowAdminMeny"] is true;

        controller.ViewData["ShowAdminMeny"] =
            state?.AdminMenyEnabled == true && explicitlyRequested;
    }
}
