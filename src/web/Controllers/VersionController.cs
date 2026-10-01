using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Services;

namespace Plandokument.Controllers;

public class VersionController : BasController
{
    public VersionController(GeneralInfoService generalInfoService) : base(generalInfoService) { }


    [HttpGet("dokument/version", Name = "Version")]
    [HttpGet("dokument/information")]
    [HttpGet("dokument/info")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index()
    {
        return View("Views/Version.cshtml");
    }
}
