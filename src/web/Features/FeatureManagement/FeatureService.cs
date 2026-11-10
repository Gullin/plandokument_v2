using Microsoft.Extensions.Options;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Application.Options.Application;
using Plandokument.Features.FeatureManagement.Enums;

namespace Plandokument.Features.FeatureManagement.Services;

public sealed class FeatureService(IOptions<FeatureOptions> options) : IFeatureService
{
    private const string CookiePrefix = "Feature_";

    public bool IsEnabled(HttpContext httpContext, string featureName)
    {
        // Explicit query-string override always wins. This is useful both for
        // activation when cookies cannot be persisted and for testing a
        // released feature with ?feature=0.
        var queryValue = httpContext.Request.Query[featureName].FirstOrDefault();

        if (queryValue == "1")
            return true;

        if (queryValue == "0")
            return false;

        var lifecycle = GetLifecycle(featureName);

        // A released feature no longer requires beta activation.
        if (lifecycle == FeatureLifecycle.Released)
            return true;

        // Beta features require explicit activation in this browser.
        return httpContext.Request.Cookies.TryGetValue(
                   CookiePrefix + featureName,
                   out var value)
               && value == "1";
    }

    public void SetEnabled(HttpContext httpContext, string featureName, bool enabled)
    {
        httpContext.Response.Cookies.Append(
            CookiePrefix + featureName,
            enabled ? "1" : "0",
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = httpContext.Request.IsHttps,
                Path = "/"
            });
    }

    public FeatureLifecycle GetLifecycle(string featureName)
    {
        return options.Value.States.TryGetValue(featureName, out var state)
            ? state
            : FeatureLifecycle.Beta;
    }
}
