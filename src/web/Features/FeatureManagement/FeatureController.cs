using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Controllers;
using Plandokument.Features.FeatureManagement.Entities;

namespace Plandokument.Features.FeatureManagement.Controllers;

[Route("beta")]
public sealed class FeatureController(IFeatureService featureService) : Controller
{
    [HttpGet("admin/meny")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult EnableSidebar()
    {
        featureService.SetEnabled(
            HttpContext,
            FeatureNames.AdminMeny,
            enabled: true);

        // The query parameter is also supplied on the redirect. This means
        // activation works for the current request even when the browser
        // cannot persist cookies.
        //return RedirectToAction("Index", "Home", new { sidebar = 1 });

        //return Redirect("/dokument?sidebar=1");

        return RedirectToAction(
            nameof(DokumentController.Dokument),
            "Dokument",
            new { AdminMeny = 1 });
    }

    [HttpGet("admin/meny/off")]
    [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult DisableSidebar()
    {
        featureService.SetEnabled(
            HttpContext,
            FeatureNames.AdminMeny,
            enabled: false);

        //return RedirectToAction("Index", "Home", new { sidebar = 0 });

        //return Redirect("/dokument?sidebar=0");

        return RedirectToAction(
            nameof(DokumentController.Dokument),
            "Dokument",
            new { AdminMeny = 0 });
    }
}
