using Microsoft.Extensions.Options;
using Plandokument.Application.Options.Application;
using Plandokument.Models;
using System.Diagnostics;
using System.Reflection;

namespace Plandokument.Application.Services;

public class GeneralInfoService
{
    private readonly string _version;
    private string _versionSuffix;
    private readonly string _versionInfo;
    private readonly string _copyright;
    private string _gitCommitSha;
    private string _gitCommitShaShort;
    private readonly IOptions<ApplicationSettings> _applicationSettings;
    private readonly ILogger<GeneralInfoService> _logger;

    public GeneralInfoService(
        IOptions<ApplicationSettings> applicationSettings,
        ILogger<GeneralInfoService> logger
        )
    {
        _applicationSettings = applicationSettings;
        _logger = logger;

        // Assembly.GetExecutingAssembly() - assembly koden ligger i
        // Assembly.GetEntryAssembly() - projektets entry point, ex. Program.cs
        Assembly? assembly = Assembly.GetEntryAssembly();
        if (assembly is null)
        {
            _logger.LogWarning("Assembly.GetEntryAssembly() returned null. Falling back to Assembly.GetExecutingAssembly().");
            assembly = Assembly.GetExecutingAssembly();
        }
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

        _gitCommitSha = assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(x => x.Key == "GitCommitSha")
            ?.Value ?? string.Empty;

        _gitCommitShaShort = GetShortSha(_gitCommitSha);

        // Use the version from the application settings if it's provided, otherwise use the product version from the assembly.
        if (String.IsNullOrWhiteSpace(_applicationSettings.Value.Version))
        {
            _version = fvi.ProductVersion ?? "0.0.0";
            _versionInfo = _version;
        }
        else
        {
            _version = fvi.ProductVersion ?? "0.0.0";
            _versionInfo = _version;
        }

        if (!String.IsNullOrWhiteSpace(_applicationSettings.Value.VersionSuffix))
        {
            _versionSuffix = _applicationSettings.Value.VersionSuffix;
            _versionInfo += "-" + _versionSuffix;
        }

        if (!String.IsNullOrWhiteSpace(_gitCommitShaShort))
        {
            _versionInfo += "+sha." + _gitCommitShaShort;
        }



        var year = DateTime.Now.Year.ToString();
        _copyright = string.IsNullOrWhiteSpace(fvi.LegalCopyright) ? "© " + year : fvi.LegalCopyright + " - " + year;
    }

    private static string GetShortSha(string? sha, int length = 7)
    {
        if (string.IsNullOrWhiteSpace(sha))
            return string.Empty;

        return sha.Length <= length
            ? sha
            : sha[..length];
    }

    public GeneralInfo GetGeneralInfo() => new GeneralInfo
    {
        Version = _version,
        VersionSuffix = _versionSuffix,
        VersionInfo = "v" + _versionInfo,
        Copyright = _copyright,
        GitCommitSha = _gitCommitSha,
        GitCommitShaShort = _gitCommitShaShort,
        SearchParams = _applicationSettings.Value.SearchParams
    };
}
