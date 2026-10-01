using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Services;

namespace Plandokument.Controllers
{
    public class KontrollpanelController : BasController
    {
        public KontrollpanelController(GeneralInfoService generalInfoService) : base(generalInfoService) { }


        [HttpGet("dokument/kontrollpanel", Name = "Kontrollpanel")]
        [HttpGet("dokument/admin", Name = "Admin")]
        [HttpGet("dokument/administration")]
        [HttpGet("dokument/dashboard")]
        [HttpGet("dokument/system")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Policy = "AdOrUserAdmins")]
        public IActionResult Index()
        {
            return View("Views/Kontrollpanel.cshtml");
        }
    }
}
