using Plandokument.Features.FeatureManagement.Enums;

namespace Plandokument.Application.Interfaces.Services;

public interface IFeatureService
{
    bool IsEnabled(HttpContext httpContext, string featureName);
    void SetEnabled(HttpContext httpContext, string featureName, bool enabled);
    FeatureLifecycle GetLifecycle(string featureName);
}
