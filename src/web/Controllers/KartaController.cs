using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Services;

namespace Plandokument.Controllers;

public class KartaController : BasController
{
    public KartaController(GeneralInfoService generalInfoService) : base(generalInfoService) { }


    [HttpGet("dokument/karta", Name = "Karta")]
    [HttpGet("dokument/kartor")]
    [HttpGet("dokument/map")]
    [HttpGet("dokument/maps")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index()
    {
        return View("Views/Karta.cshtml");
    }
}
