using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Services;

namespace Plandokument.Controllers;

public class OmController : BasController
{
    public OmController(GeneralInfoService generalInfoService) : base(generalInfoService) { }


    [HttpGet("dokument/om", Name = "Om")]
    [HttpGet("dokument/help")]
    [HttpGet("dokument/hjalp")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index()
    {
        return View("Views/Om.cshtml");
    }
}
