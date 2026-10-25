using Microsoft.AspNetCore.Mvc;
using Plandokument.Application.Services;
using Plandokument.Models;

namespace Plandokument.Controllers;

public class BasController : Controller
{
    private readonly GeneralInfoService _generalInfoService;
    protected ViewResults result;

    [ViewData]
    public string Version { get; set; }
    [ViewData]
    public string VersionSuffix { get; set; }
    [ViewData]
    public string VersionInfo { get; set; }
    [ViewData]
    public string Copyright { get; set; }
    [ViewData]
    public string GitCommitSHA { get; set; }
    [ViewData]
    public string GitCommitSHAShort { get; set; }

    protected BasController(GeneralInfoService generalInfoService)
    {
        _generalInfoService = generalInfoService;

        result = new ViewResults
        {
            GeneralInfo = _generalInfoService.GetGeneralInfo()
        };

        Version = result.GeneralInfo.Version;
        VersionSuffix = result.GeneralInfo.VersionSuffix;
        VersionInfo = result.GeneralInfo.VersionInfo;
        Copyright = result.GeneralInfo.Copyright;
        GitCommitSHA = result.GeneralInfo.GitCommitSha;
        GitCommitSHAShort = result.GeneralInfo.GitCommitShaShort;

        
    }
}
