using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Application.Options.PlanDocumentFiles;
using Plandokument.Domain.Entities;
using Plandokument.Domain.Search;
using Plandokument.Infrastructure.Cache;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Plandokument.Application.Services;

public class PlanDocumentsService : IPlanDocumentsService
{
    private readonly PlanDocumentFiles _options;
    private readonly IPlanDocumentsCacheService _planDocumentsCacheService;
    private readonly IPlanBasCacheService _planBasCacheService;
    private readonly IDocumentTypeCacheService _documentTypeCacheService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<PlanDocumentsService> _logger;

    public PlanDocumentsService(
        IOptions<PlanDocumentFiles> options,
        IPlanDocumentsCacheService planDocumentsCacheService,
        IPlanBasCacheService planBasCacheService,
        IDocumentTypeCacheService documentTypeCacheService,
        IWebHostEnvironment env,
        ILogger<PlanDocumentsService> logger)
    {
        _options = options.Value;
        _planDocumentsCacheService = planDocumentsCacheService;
        _planBasCacheService = planBasCacheService;
        _documentTypeCacheService = documentTypeCacheService;
        _env = env;
        _logger = logger;
    }


    /// <summary>
    /// Returnerar alla plandokument i cachen
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<PlanDocuments>> GetAllDocumentsAsync()
    {
        return await _planDocumentsCacheService.GetOrRefreshAsync();
    }



    /// <summary>
    /// Returnerar plandokument med planinformation för alla planer
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<PlanFileResult>> GetAllDocumentsWithPlansAsync()
    {
        return await PlansDocsAsync(
            _planBasCacheService
                .GetOrRefreshAsync()
                .Result
                .Select(p => p.Register.plan_id)
                .ToList()
            );
    }


    /// <summary>
    /// returnerar plandokument med planinformation för angivna planer och dokumenttyp
    /// </summary>
    /// <param name="planIds"></param>
    /// <param name="documentType"></param>
    /// <returns></returns>
    public async Task<IEnumerable<PlanFileResult>> GetDocumentsBySpecificPlansAsync(List<string> planIds, string documentType = "dokument")
    {
        return await PlansDocsAsync(planIds, documentType);
    }


    /// <summary>
    /// Returnerar plandokument för angivna planer och dokumenttyp
    /// </summary>
    /// <param name="planIds">Lista med plannycklar</param>
    /// <param name="documentType">Vilken typ av dokument. Standard och alla är 'dokument'</param>
    /// <returns></returns>
    private async Task<List<PlanFileResult>> PlansDocsAsync(List<string> planIds, string documentType = "dokument")
    {
        var searchedPlans = await FilterPlanBasisCacheAsync(planIds);
        var documentTypes = await _documentTypeCacheService.GetOrRefreshAsync();
        var allDocuments = new List<PlanFileResult>();

        string documentPrefix = "DP";

        foreach (var plan in searchedPlans)
        {
            //string documentBaseName = plan.Register.lmakt?.Replace('/', '_') ?? "";
            string[] documentBaseNames = new string[]
            {
                plan.Register.lmakt?.Replace('/', '_') ?? "",
                plan.Register.egn_akt?.ToUpper().Replace("1282K-", "") ?? ""
            };

            if (documentBaseNames == null || documentBaseNames.All(string.IsNullOrWhiteSpace))
                continue;

            IEnumerable<DocumentType> typesToSearch =
                documentType switch
                {
                    "handling" => documentTypes.Where(d => d.IsPlanhandling && !d.Type.IsNullOrEmpty()),
                    "dokument" => documentTypes.Where(d => !d.Type.IsNullOrEmpty()),
                    _ => documentTypes.Where(d => d.UrlFilter == documentType && !d.Type.IsNullOrEmpty())
                };

            foreach (var documentBaseName in documentBaseNames)
            {
                foreach (var type in typesToSearch)
                {
                    string suffix = string.IsNullOrEmpty(type.Suffix) ? "" : "_" + type.Suffix;
                    string searchedFile = documentPrefix + documentBaseName + suffix;

                    var files = await FindFileInCacheAsync(searchedFile, plan.Register.plan_id, documentPrefix + documentBaseName);
                    allDocuments.AddRange(files);
                }
            }
        }

        AddThumbnails(allDocuments);

        return allDocuments;
    }



    /// <summary>
    /// Filtrerar planbas-cache för att endast returnera de planer som efterfrågas.
    /// </summary>
    /// <param name="planIds">Lista med resp. plans nyckel</param>
    /// <returns></returns>
    private async Task<List<PlanBas>> FilterPlanBasisCacheAsync(List<string> planIds)
    {
        var cachedPlans = await _planBasCacheService.GetOrRefreshAsync();
        return cachedPlans.Where(p => planIds.Contains(p.Register.plan_id)).ToList();
    }


    /// <summary>
    /// Söker upp filer i cache över plandokument baserat på söksträng
    /// </summary>
    /// <param name="searchedFile">Sökt dokument</param>
    /// <param name="planId">Plans nyckel</param>
    /// <param name="dokumentAkt"></param>
    /// <returns></returns>
    private async Task<IEnumerable<PlanFileResult>> FindFileInCacheAsync(string searchedFile, string planId, string dokumentAkt)
    {
        var regEx = SearchFilter(searchedFile);
        var files = await _planDocumentsCacheService.GetOrRefreshAsync();

        var result = new List<PlanFileResult>();
        foreach (var file in files)
        {
            var matches = file.Documents?
                .Where(f => regEx.IsMatch(f.Name))
                .ToList() ?? new List<FileInfo>();

            foreach (var fi in matches)
            {
                result.Add(await CreateFileResultAsync(planId, dokumentAkt, fi, file.RootPath, file.VirtualPath));
            }
        }

        return result;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="searchedFile"></param>
    /// <returns></returns>
    private Regex SearchFilter(string searchedFile)
    {
        var extensions = _options.FileTypes ?? Array.Empty<string>();
        //var extensions = ConfigurationManager.AppSettings["fileExtentions"]?.Split(',') ?? Array.Empty<string>();
        string extFilter = extensions.Length == 0
            ? @"(\.[0-9a-öA-Ö]+)"
            : string.Join("|", extensions.Select(e => "\\" + e.ToLower() + "|" + "\\" + e.ToUpper()));

        string filter = $"{searchedFile}(,[0-9a-öA-Ö]+)*({extFilter})";
        return new Regex(filter);
    }


    /// <summary>
    /// Skapar ett PlanFileResult-objekt från en FileInfo som hittats i cachen som del av resultatobjektet
    /// </summary>
    /// <param name="planId">Planens nyckel</param>
    /// <param name="dokumentAkt"></param>
    /// <param name="fi"></param>
    /// <param name="rootPath"></param>
    /// <param name="virtualRootPath"></param>
    /// <returns></returns>
    private async Task<PlanFileResult> CreateFileResultAsync(string planId, string dokumentAkt, FileInfo fi, string rootPath, string virtualRootPath)
    {
        // filnamn kan ha flera _ som delar av informationsmängder, sista avgör dokumenttypen
        //string[] parts = Path.GetFileNameWithoutExtension(
        //                        fi.Name
        //                    )
        //                    .Replace(dokumentAkt, "")
        //                    .Split('_');
        string[] parts = fi.Name
                            .Replace(dokumentAkt, "")
                            .Split('_');
        string lastPart = parts.LastOrDefault() ?? "";

        string potentialType = new string(lastPart.ToCharArray()
                                .Reverse()
                                .ToArray())
                                .Substring(
                                            (fi.Extension.Length),
                                            (lastPart.Length - fi.Extension.Length));
        potentialType = new string(potentialType.ToCharArray().Reverse().ToArray());

        //var (findType, documentTypePart) = await DetermFindTypeAsync(potentialType);
        string[] types = await DetermFindTypeAsync(potentialType);

        string documentType = await GetDocumentTypeAsync(types[0]);

        var filePathPart = fi.DirectoryName?
            .Replace("\\", "/")
            .Replace(rootPath, "")
            .Replace("\\", "/") ?? "";


        return new PlanFileResult
        {
            Path = rootPath,
            PathVirtual = (virtualRootPath + filePathPart),
            Name = fi.Name,
            Extension = fi.Extension,
            Size = fi.Length,
            PlanId = planId,
            //DocumentType = types[0],
            DocumentType = documentType,
            FindType = types[2],
            DocumentPart = types[1],
            ThumbnailPath = "",
            ThumbnailIndication = "N/A"
        };
    }

    /// <summary>
    /// Avgör filens dokumenttyp och dokumentdel samt sätter om dokumentet är delat, exakt eller ohanterat genom filnamnets del av namnkonventionen efter sista understrecket, exv. ..._beskrivning,1
    /// </summary>
    /// <param name="potentialDocumentType">Resten av filnamnet efter bortklippt planbeteckning</param>
    /// <returns>Arrary innehållande [0] = DocumentType, [1] = DocumentPart, [2] = FindType enl. exv. _[besk],[A]</returns>
    //private async Task<(string findType, string documentTypePart)> DetermFindTypeAsync(string potentialDocumentType)
    private async Task<string[]> DetermFindTypeAsync(string potentialDocumentType)
    {
        // Hanterar och kontrollerar för flera dokumentdelar
        string findtype = string.Empty;
        string findtypePart = string.Empty;
        string[] types = new string[3];
        //[0] = DocumentType
        //[1] = DocumentPart
        //[2] = FindType
        try
        {
            if (potentialDocumentType.Contains(","))
            {
                string[] findtypeParts = potentialDocumentType.Split(',');

                // Följer ej namnkonventionen, för många möjligheter till dokumentdelar
                if (findtypeParts.Length > 2)
                {
                    findtype = FindTypes.Unmanaged;
                    types[2] = FindTypes.Unmanaged;
                    throw new Exception("För många signaler om dokumentdelar. Kommatecken får utelämnas eller endast förekomma en gång.");
                }
                else if (findtypeParts.Length == 2)
                {
                    if (string.IsNullOrWhiteSpace(findtypeParts[1]))
                    {
                        findtype = FindTypes.IsPart;
                        types[0] = findtypeParts[0];
                        types[2] = FindTypes.IsPart;
                    }
                    else if (!string.IsNullOrWhiteSpace(findtypeParts[1]))
                    {
                        findtypePart = findtypeParts[1].ToString();
                        findtype = FindTypes.IsPart;
                        types[0] = findtypeParts[0];
                        types[1] = findtypeParts[1];
                        types[2] = FindTypes.IsPart;
                    }
                    else
                    {
                        findtype = FindTypes.Unmanaged;
                        types[2] = FindTypes.Unmanaged;
                    }
                }
                else
                {
                    throw new Exception("Innehåller kommatecken som signal om dokumentdelar, men kan inte hantera formen.");
                }

                findtypePart = findtypeParts[0];
            }
            else
            {
                findtype = FindTypes.Exact;
                findtypePart = potentialDocumentType;
                types[0] = potentialDocumentType;
                types[2] = FindTypes.Exact;
            }
        }
        catch (Exception ex)
        {

            // Klassens namn för loggning
            string className = this.GetType().Name;
            // Metod i klassen som används
            string methodName = MethodBase.GetCurrentMethod().Name;

            throw;
        }

        //return (findtype, findtypePart);
        return types;
    }


    /// <summary>
    /// Returnerar dokumenttyp i klartext baserat på suffix i filnamnet.
    /// </summary>
    /// <param name="suffix">Filsuffixet i namnkonventionen som indikerar dokumenttyp</param>
    /// <returns> Exv. inget suffix returnerar Karta, ovr eller handling = Övriga, besk = Beskrivning osv.</returns>
    private async Task<string> GetDocumentTypeAsync(string suffix)
    {
        if (string.IsNullOrEmpty(suffix))
            return "Karta";

        var docTypes = await _documentTypeCacheService.GetOrRefreshAsync(); ;
        var match = docTypes.FirstOrDefault(d => d.Suffix == suffix);
        if (match != null)
            return match.Type;

        if (suffix == "ovr" || suffix == "handling")
            return "Övriga";

        return "";
    }

    /// <summary>
    /// Letar efter eventuella thumbnails till plandokument (t.ex. .tif-filer).
    /// Letar i angivna root paths och konstant katalog "auto-miniatyrbild-plankarta".
    /// </summary>
    private void AddThumbnails(List<PlanFileResult> results)
    {
        try
        {
            var rootPaths = _options.Paths; // string[]
            if (rootPaths == null || rootPaths.Length == 0)
                return;

            // Loopa över alla möjliga root paths
            foreach (var doc in results)
            {
                if (doc.DocumentType == "Karta" &&
                    doc.Extension.ToLower().Equals(".tif", StringComparison.OrdinalIgnoreCase))
                {
                    string baseName = Path.GetFileNameWithoutExtension(doc.Name);
                    string filter = baseName + "_thumnail-*.jpg";
                    Regex regex = new Regex(
                        $"({baseName}_thumnail-l.jpg)|({baseName}_thumnail-s.jpg)",
                        RegexOptions.IgnoreCase);

                    bool found = false;

                    foreach (var rootPath in rootPaths)
                    {
                        if (string.IsNullOrWhiteSpace(rootPath.Physical))
                            continue;

                        //string thumbRoot = rootPath.Virtual.Replace(@"\\", "/").Replace(@"\", "/").TrimEnd('/') + "/" + rootPath.ThumbnailSubFolder.Replace(@"\\", "/").Replace(@"\", "/").TrimStart('/');
                        string thumbRoot = Path.Combine(rootPath.Physical,rootPath.ThumbnailSubFolder);

                        string resolvedPath;
                        if (Path.IsPathRooted(thumbRoot))
                            resolvedPath = thumbRoot;
                        else
                            resolvedPath = Path.Combine(_env.WebRootPath, thumbRoot);

                        var thumbDir = new DirectoryInfo(resolvedPath);
                        if (!thumbDir.Exists)
                            continue;

                        List<FileInfo> foundThumbs = thumbDir
                            .EnumerateFiles(filter)
                            .Where(f => regex.IsMatch(f.FullName))
                            .ToList();

                        bool small = foundThumbs.Any(f => f.Name.Contains("-s"));
                        bool large = foundThumbs.Any(f => f.Name.Contains("-l"));

                        if (small || large)
                        {
                            doc.ThumbnailIndication = small && large ? "s,l" : (small ? "s" : "l");
                            //doc.ThumbnailPath = Path.Combine(thumbRoot).Replace("\\", "/") + "/";
                            doc.ThumbnailPath = rootPath.Virtual.Replace(@"\\", "/").Replace(@"\", "/").TrimEnd('/') + "/" + rootPath.ThumbnailSubFolder.Replace(@"\\", "/").Replace(@"\", "/").TrimStart('/');
                            found = true;
                            break; // sluta söka i fler rootPaths
                        }
                    }

                    if (!found)
                        doc.ThumbnailIndication = "N/A";
                }
                else
                {
                    doc.ThumbnailIndication = "N/A";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hantering av miniatyrbilder: {Message}", ex.Message);

            foreach (var doc in results)
                doc.ThumbnailIndication = "N/A";
        }
    }
}
