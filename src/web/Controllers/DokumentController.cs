using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Options.Application;
using Plandokument.Application.Services;
using Plandokument.Domain.Entities;
using Plandokument.Domain.Search;
using Plandokument.Infrastructure.Logging;
using System.Diagnostics;

namespace Plandokument.Controllers;

public class DokumentController : BasController
{
    private readonly Search _optionsSearch;
    private readonly IPlanBasCacheService _planBasCacheService;
    private readonly IPlanBerorFastighetCacheService _planBerorFastighetCacheService;
    private readonly ILogger _dbRequestStatisticsLogger;

    public DokumentController(GeneralInfoService generalInfoService, IOptions<ApplicationSettings> options,
        IPlanBasCacheService planBasCacheService,
        IPlanBerorFastighetCacheService planBerorFastighetCacheService,
        ILoggerFactory loggerFactory
        ) : base(generalInfoService)
    {
        _optionsSearch = options.Value.SearchParams;
        _planBasCacheService = planBasCacheService;
        _planBerorFastighetCacheService = planBerorFastighetCacheService;
        _dbRequestStatisticsLogger = loggerFactory.CreateLogger("RequestStatistics");

        // ViewData kan inte sättas i konstruktorn (livscykelproblem). HttpContext finns inte ännu och MVC har inte kopplat ViewData eller ControllerContext.
    }



    // Alternativ för att sätta viewdata innan varje action är genom ViewData-attribut på properties
    //public override void OnActionExecuting(ActionExecutingContext context)
    //{
    //    ViewData["VersionInfo"] = result.GeneralInfo.VersionInfo;
    //    ViewData["Copyright"] = result.GeneralInfo.Copyright;

    //    base.OnActionExecuting(context);
    //}



    [HttpGet("dokument")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Dokument()
    {
        var query = HttpContext.Request.Query;
        string? q = query[_optionsSearch.UrlParameterSearchString];
        string? dokument = query[_optionsSearch.UrlParameterDocumentType];
        string? type = query[_optionsSearch.UrlParameterSearchType];


        // Om kvp-parametern för sökning existerar → visa inte standard tom sida
        if (!string.IsNullOrEmpty(q))
        {
            var timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var stopWatch = Stopwatch.StartNew();

            result.IsSearched = true;
            result.SearchTerms = q.Split(
                _optionsSearch.URLQueryStringSeparator,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            ).Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
            result.SearchDocumentType = string.IsNullOrWhiteSpace(dokument) ? null : dokument;
            result.SearchType = string.IsNullOrWhiteSpace(type) ? null : type;

            result.Plans = await SearchDocumentAsync(result.SearchTerms, result.SearchType);


            // Loggar sökstatistik till databasen
            stopWatch.Stop();
            _dbRequestStatisticsLogger.LogInformation(
                "RequestStatistic: when={when}, nbr_search={nbr_search}, nbr_hits={nbr_hits}, searchtime={searchtime}",
                timeStamp,
                result.SearchTerms.Length,
                result.Plans.Count,
                stopWatch.ElapsedMilliseconds
            );

            result.Message = "KVP sökning";
            return View("Views/Dokument.cshtml", result);
        }



        result.Message = "Start";
        return View("Views/Dokument.cshtml", result);
    }



    [HttpGet("/dokument/alla", Name = "DokumentAlla")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult DokumentAlla()
    {
        result.Message = "Alla";

        //TODO: ta fram alla

        return View("Views/Alla.cshtml", result);
    }



    [HttpGet("dokument/{query}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> DokumentQueryAsync(string query)
    {
        var timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var stopWatch = Stopwatch.StartNew();

        result.IsSearched = true;
        result.SearchTerms = query.Split(
                _optionsSearch.URLQueryStringSeparator,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            ).Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        result.Plans = await SearchDocumentAsync(result.SearchTerms, null);

        // Loggar sökstatistik till databasen
        stopWatch.Stop();
        _dbRequestStatisticsLogger.LogInformation(
            "RequestStatistic: when={when}, nbr_search={nbr_search}, nbr_hits={nbr_hits}, searchtime={searchtime}",
            timeStamp,
            result.SearchTerms.Length,
            result.Plans.Count,
            stopWatch.ElapsedMilliseconds
        );

        result.Message = "Allmän sökning";
        return View("Views/Dokument.cshtml", result);
    }



    // signals kan vara en eller flera signaler separerade med komma
    // För att inte fångas upp av statiska filer (css, js, images, lib) så används en regex constraint, i annat fall uppstår "catch-all rout".
    // Alla underkataloger under wwwroot som inte ska tolkas som signaler måste läggas till i regexen.
    [HttpGet("{signals:regex(^(?!css|js|images|lib|api|plandokument|zipTemp).*)}/{*query}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> DokumentSearch(string signals, string query)
    {
        if (query is null) return NoContent();

        var timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var stopWatch= Stopwatch.StartNew();

        result.IsSearched = true;
        var parts = signals.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string? primarySignal = parts.ElementAtOrDefault(0);
        string? secondarySignal = parts.ElementAtOrDefault(1);

        result.SearchTerms = query.Split(
                _optionsSearch.URLQueryStringSeparator,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            ).Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        result.SearchDocumentType = primarySignal;
        result.SearchType = secondarySignal;

        result.Plans = await SearchDocumentAsync(result.SearchTerms, secondarySignal);

        // Loggar sökstatistik till databasen
        stopWatch.Stop();
        _dbRequestStatisticsLogger.LogInformation(
            "RequestStatistic: when={when}, nbr_search={nbr_search}, nbr_hits={nbr_hits}, searchtime={searchtime}",
            timeStamp,
            result.SearchTerms.Length,
            result.Plans.Count,
            stopWatch.ElapsedMilliseconds
        );

        result.Message = "Avancerad sökning";
        return View("Views/Dokument.cshtml", result);
    }




    private async Task<List<PlanSearchResult>> SearchDocumentAsync(IEnumerable<string> searchedPlans, string begrepp)
    {
        if (searchedPlans == null || !searchedPlans.Any())
            return new List<PlanSearchResult>();

        begrepp = begrepp?.ToUpper() ?? string.Empty;

        var plans = await _planBasCacheService.GetOrRefreshAsync();
        var planBerorFastighet = (string.IsNullOrWhiteSpace(begrepp) || begrepp is "FASTIGHET" or "FASTIGHETNYCKEL")
            ? await _planBerorFastighetCacheService.GetOrRefreshAsync()
            : new List<PlanBerorFastighet>();



        #region skriver ut Plan Bas Cache som hjälp vid utveckling
        //TODO: Cache, PlanBasCache, Flytta till Admin-funktion
        //TODO: Cache, Möjlighet att skriva ut alla cacher
        // 🔹 Ange filens sökväg (lägg i appens loggmapp)

        //var logDirectory = "log";
        //if (!Directory.Exists(logDirectory))
        //    Directory.CreateDirectory(logDirectory);
        //var logFile = Path.Combine(logDirectory, "plan_cache_dump.txt");

        //// 🔹 Skriv ut valda fält
        //await using (var writer = new StreamWriter(logFile, append: false)) // append:false för att skriva om filen varje gång
        //{
        //    await writer.WriteLineAsync("plan_id;lmakt;egn_akt;akt_pb");

        //    foreach (var plan in plans)
        //    {
        //        var line = $"{plan.Register.plan_id};{plan.Register.lmakt};{plan.Register.egn_akt};{plan.Geometri.akt_pb}";
        //        await writer.WriteLineAsync(line);
        //    }
        //}
        #endregion



        var parcelBlockUnitSearchSign = _optionsSearch.URLParcelBlockUnitSign;
        var result = new List<PlanSearchResult>();

        foreach (var potentialPlan in searchedPlans)
        {
            IEnumerable<PlanBas> queryResult = Enumerable.Empty<PlanBas>();

            if (!string.IsNullOrWhiteSpace(begrepp))
            {
                queryResult = begrepp switch
                {
                    "FASTIGHET" => from p in plans
                                   join f in planBerorFastighet
                                   on p.Register.plan_id equals f.nyckel
                                   where f.fastighet.Equals(
                                       potentialPlan.ToUpper().Replace(parcelBlockUnitSearchSign, ":"),
                                       StringComparison.OrdinalIgnoreCase)
                                   select p,

                    "FASTIGHETNYCKEL" when int.TryParse(potentialPlan, out var val) =>
                        from p in plans
                        join f in planBerorFastighet on p.Register.plan_id equals f.nyckel
                        where f.nyckel_fastighet == val
                        select p,

                    "AKTTIDIGARE" => plans.Where(p =>
                        string.Equals(p.Register.egn_akt,
                        potentialPlan.Contains("1282k-") ? potentialPlan : "1282k-" + potentialPlan,
                        StringComparison.OrdinalIgnoreCase)),

                    "AKT" => plans.Where(p =>
                        string.Equals(p.Register.lmakt,
                        potentialPlan,
                        StringComparison.OrdinalIgnoreCase)),

                    "AKTEGEN" => plans.Where(p =>
                        string.Equals(p.Geometri.akt_pb ?? "",
                        potentialPlan,
                        StringComparison.OrdinalIgnoreCase)),

                    "NYCKEL" => plans.Where(p =>
                        string.Equals(p.Register.plan_id,
                        potentialPlan,
                        StringComparison.OrdinalIgnoreCase)),

                    //TODO: Sökning, Begrepp, Default case (ska inte inträffa) men kan returnera felmeddelande
                    //_ => plans.Where(p =>
                    //    string.Equals(p.GetType().GetProperty(begrepp)?.GetValue(p)?.ToString() ?? "",
                    //    potentialPlan, StringComparison.OrdinalIgnoreCase))
                };
            }
            else
            {
                // Wildcard-sökning mot flera kolumner
                queryResult = plans.Where(p =>
                {
                    var aktPb = p.Geometri.akt_pb ?? "";
                    var normalizedSearch = potentialPlan.Contains("1282k-")
                        ? potentialPlan
                        : "1282k-" + potentialPlan;

                    return string.Equals(p.Register.plan_id, potentialPlan, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(p.Register.lmakt, potentialPlan, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(aktPb, potentialPlan, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(p.Register.egn_akt, normalizedSearch, StringComparison.OrdinalIgnoreCase);
                });
            }

            // Lägg till i resultatet
            if (queryResult.Any())
            {
                result.AddRange(queryResult.Select(row => new PlanSearchResult
                {
                    Nyckel = row.Register.plan_id,
                    Akt = row.Register.lmakt,
                    AktEgen = row.Register.egn_akt,
                    AktPb = row.Geometri.akt_pb,
                    PlanFk = row.Register.planfk,
                    PlanNamn = row.Register.plannamn,
                    IsGenomf = Convert.ToBoolean(row.Register.isgenomf),
                    Begrepp = begrepp,
                    SearchedString = potentialPlan
                }));
            }
            else
            {
                result.Add(new PlanSearchResult
                {
                    Begrepp = begrepp,
                    SearchedString = potentialPlan
                });
            }
        }

        // Ta bort dubbletter och slå ihop begrepp
        List<PlanSearchResult> distinctResults = await Task.Run(() =>
            result
                .GroupBy(r => r.Nyckel ?? r.SearchedString)
                .Select(g =>
                {
                    var first = g.First();
                    first.Begrepp = string.Join(", ", g.Select(x => x.Begrepp).Distinct());
                    first.SearchedString = string.Join(", ", g.Select(x => x.SearchedString).Distinct());
                    return first;
                })
                .ToList());

        return distinctResults;
    }

}
