using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Services;
using Plandokument.Models;

namespace Plandokument.Controllers;

public class BasController : Controller
{
    private readonly GeneralInfoService _generalInfoService;
    protected ViewResults result;

    [ViewData]
    public string VersionInfo { get; set; }
    [ViewData]
    public string Copyright { get; set; }

    protected BasController(GeneralInfoService generalInfoService)
    {
        _generalInfoService = generalInfoService;

        result = new ViewResults
        {
            GeneralInfo = _generalInfoService.GetGeneralInfo()
        };

        VersionInfo = result.GeneralInfo.VersionInfo;
        Copyright = result.GeneralInfo.Copyright;
        
    }
}
