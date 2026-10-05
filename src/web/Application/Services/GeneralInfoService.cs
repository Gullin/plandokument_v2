using Microsoft.Extensions.Options;
using Plandokument.Application.Options.Application;
using Plandokument.Models;
using System.Diagnostics;
using System.Reflection;

namespace Plandokument.Application.Services;

public class GeneralInfoService
{
    private readonly string _versionInfo;
    private readonly string _copyright;
    private readonly IOptions<ApplicationSettings> _applicationSettings;

    public GeneralInfoService(IOptions<ApplicationSettings> applicationSettings)
    {
        _applicationSettings = applicationSettings;

        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

        if (String.IsNullOrWhiteSpace(_applicationSettings.Value.Version))
        {
            _versionInfo = fvi.ProductVersion ?? "0.0.0";
        }
        else
        {
            _versionInfo = _applicationSettings.Value.Version;
        }
        if (!String.IsNullOrWhiteSpace(_applicationSettings.Value.VersionSuffix))
        {
            _versionInfo += "-" + _applicationSettings.Value.VersionSuffix;
        }


        var year = DateTime.Now.Year.ToString();
        _copyright = string.IsNullOrWhiteSpace(fvi.LegalCopyright) ? "© " + year : fvi.LegalCopyright + " - " + year;
    }

    public GeneralInfo GetGeneralInfo() => new GeneralInfo
    {
        VersionInfo = "v" + _versionInfo,
        Copyright = _copyright,
        SearchParams = _applicationSettings.Value.SearchParams
    };
}
