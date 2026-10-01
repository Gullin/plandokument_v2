using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Logging;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Interfaces.Repositories;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Application.Options.Application;
using Plandokument.Application.Options.Caches;
using Plandokument.Application.Options.Logging;
using Plandokument.Application.Options.PlanDocumentFiles;
using Plandokument.Application.Services;
using Plandokument.Controllers.Api;
using Plandokument.Domain.Interfaces;
using Plandokument.Infrastructure;
using Plandokument.Infrastructure.Background;
using Plandokument.Infrastructure.Cache;
using Plandokument.Infrastructure.Logging;
using Plandokument.Infrastructure.Process;
using Plandokument.Infrastructure.Repositories;
using Plandokument.Infrastructure.Security;
using Plandokument.Middleware;
using System.IO;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);



// ########## Loggning
builder.Logging.ClearProviders();
//builder.Logging.AddProvider(
//    new FileGeneralLoggerProvider(
//        builder.Services.BuildServiceProvider().GetRequiredService<IOptions<ApplicationSpecificLoggingSettings>>()
//        )
//    );
// Återaktiverar Logging:LogLevel från appsettings.json så att detta kan användas
builder.Logging.AddConfiguration(
    builder.Configuration.GetSection("Logging")
);
// Om endast delar av ApplicationSpecificLoggingSettings ska användas, avkommentera nedan
//builder.Services.Configure<ApplicationSpecificLoggingSettings>(
//    builder.Configuration.GetSection("Logging:ApplicationSpecificLogging"));
// Register provider types
builder.Services.AddSingleton<ILoggerProvider, FileGeneralLoggerProvider>();
builder.Services.AddSingleton<ILoggerProvider, DbStatisticLoggerProvider>();
builder.Logging.AddFilter((provider, category, logLevel) =>
{
    // Om kategorin är "RequestStatistics" -> filtrera bort allt för andra providers
    if (category == "RequestStatistics" && provider != typeof(DbStatisticLoggerProvider).FullName)
        return false;

    return true; // annars tillåt loggning
});



// ########## AppSettings/Konfiguration
builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection("Application"));
// Program.cs (before builder.Build())
var appSettings = builder.Configuration.GetSection("Application")
                    .Get<ApplicationSettings>() ?? new ApplicationSettings();
builder.Services.AddSingleton(appSettings.Authorizations);

builder.Services.Configure<CachesSettings>(
    builder.Configuration.GetSection("Caches"));

builder.Services.Configure<PlanDocumentFiles>(
    builder.Configuration.GetSection("PlanDocumentFiles"));
//var staticPathConfig = builder.Configuration.GetSection("PlanDocumentFiles").Get<PlanDocumentFiles>();



// ########## zipTemp
// Configure ApplicationSpecificLoggingSettings so FileGeneralLoggerProvider can be constructed
//builder.Services.Configure<ApplicationSpecificLoggingSettings>(
//    builder.Configuration.GetSection("Logging:ApplicationSpecificLogging"));

// Validate/create directories and log early using the registered FileGeneralLoggerProvider
// Create settings from configuration and construct the file logger provider directly
//var appLoggingSpecificSettings = builder.Configuration
//    .GetSection("Logging:ApplicationSpecificLogging")
//    .Get<ApplicationSpecificLoggingSettings>() ?? new ApplicationSpecificLoggingSettings();

//var _fileLoggerProvider = new FileGeneralLoggerProvider(Options.Create(appLoggingSpecificSettings));
//var _fileLogger = _fileLoggerProvider.CreateLogger(typeof(Program).FullName ?? "Program")
//    ?? throw new InvalidOperationException("No ILoggerProvider available for early logging.");

// Prepare validated static paths to register later when configuring middleware
//var validatedStaticPaths = new List<(string Physical, string Virtual)>();

// Ensure zipTemp exists now (so UseStaticFiles won't throw)
//var zipTempPath = Path.Combine(builder.Environment.ContentRootPath, "zipTemp");
//try
//{
//    Directory.CreateDirectory(zipTempPath);
//    _fileLogger.LogInformation("Ensured zipTemp directory exists: {Path}", zipTempPath);
//}
//catch (System.Exception ex)
//{
//    _fileLogger.LogError(ex, "Failed to ensure zipTemp directory: {Path}", zipTempPath);
//    throw;
//}

// Validate configured static paths and log/skips missing ones
//if (staticPathConfig?.Paths != null)
//{
//    foreach (var path in staticPathConfig.Paths)
//    {
//        if (string.IsNullOrWhiteSpace(path.Physical))
//        {
//            _fileLogger.LogWarning("Static path has empty Physical value, skipping (virtual={Virtual}).", path.Virtual);
//            continue;
//        }

//        var physicalPath = Path.IsPathRooted(path.Physical)
//            ? path.Physical
//            : Path.Combine(builder.Environment.ContentRootPath, path.Physical);

//        if (!Directory.Exists(physicalPath))
//        {
//            _fileLogger.LogWarning("Configured static path does not exist, skipping: {Physical} (virtual={Virtual})", physicalPath, path.Virtual);
//            continue;
//        }

//        validatedStaticPaths.Add((physicalPath, path.Virtual ?? string.Empty));
//        _fileLogger.LogInformation("Registered static path: {Physical} -> /{Virtual}", physicalPath, path.Virtual);
//    }
//}


// Ensure zipTemp directory exists at startup via hosted service
// Detta för att UseStaticFiles kräver att katalogen finns vid uppstart, annars kraschar applikationen
builder.Services.AddHostedService<ZipTempInitializer>();



// ########## Behörighet
builder.Services
    .AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();
builder.Services.AddSingleton<IAuthorizationHandler, AdOrUserAdminsHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdOrUserAdmins", policy =>
        policy.Requirements.Add(new AdOrUserAdminsRequirement()));



// ########## Cache
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IDocumentTypeRepository, FileDocumentTypeRepository>();
builder.Services.AddSingleton<IDocumentTypeService, DocumentTypeService>();
builder.Services.AddSingleton<IDocumentTypeCacheService, DocumentTypeCacheService>();

builder.Services.AddHostedService<CacheRefreshService>();

builder.Services.AddSingleton<IPlanDocumentsRepository, PlanDocumentsRepository>();
builder.Services.AddSingleton<IPlanDocumentsService, PlanDocumentsService>();
builder.Services.AddSingleton<IPlanDocumentsCacheService, PlanDocumentsCacheService>();



// Registrera egen infrastruktur (repositories, loaders, options)
builder.Services.AddInfrastructure(builder.Configuration);



// ########## Applikationsversion
builder.Services.AddSingleton<GeneralInfoService>();



// AddControllersWithViews lägger till stöd för MVC-mönstret med
// API - endpoints([ApiController], ControllerBase)
// MVC Controllers med views (Controller, ViewResult)
// Razor Views & ViewEngines
// Model binding, validation, filters etc.
builder.Services.AddControllersWithViews();

//Registrerar tjänster för endast API-controllrar (utan Views, utan Razor Pages).
//Används om inte annat än API-controllrar behövs.
//builder.Services.AddControllers();

// Registrera Swagger-generatorn
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Plandokument API", Version = "v1" });
    //options.IncludeXmlComments(Assembly.GetExecutingAssembly());
    //options.IncludeXmlComments(typeof(DocumentTypesController).Assembly);

    // Hämta den genererade XML-filen
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    // Använd XML-kommentarer i Swagger
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});





// Bygg applikationen
var app = builder.Build();

var startupLogger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("Startup");

// Loggar okända undantag globalt via middleware
// Loggar processnivå-undantag (globalt) som inte är kopplade till en HTTP-förfrågan
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
ProcessExceptionHandling.Register(loggerFactory);

// Skapar möjlighet att logga specifika fel i http-pipelinen
app.UseMiddleware<GlobalExceptionMiddleware>();



// Tar kontroll över webbapplikationens bassökväg för att hantera proxy-scenarion där applikationen inte ligger i rot av domänen.
// Exempelvis när sökväg byggts i IIS genom virtuella sökvägar.
//var basePath = builder.Configuration["Application:BasePath"];
//var pathBaseLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("PathBase");
var basePath = app.Services.GetRequiredService<IOptions<ApplicationSettings>>().Value.BasePath;

startupLogger.LogInformation("PathBase från config: {BasePath}", basePath);

if (!string.IsNullOrEmpty(basePath))
{
    app.UsePathBase(basePath);
}



// Prepare validated static paths to register later when configuring middleware
var validatedStaticPaths = new List<(string Physical, string Virtual)>();

// Ensure zipTemp exists now (so UseStaticFiles won't throw)
var zipTempRelativePath = "zipTemp";
var zipTempPath = Path.Combine(builder.Environment.ContentRootPath, zipTempRelativePath);
if (!Directory.Exists(zipTempPath))
{
    startupLogger.LogWarning("Configured static path does not exist, skipping: {Physical} (virtual={Virtual})", zipTempPath, zipTempRelativePath);
}
else
{
    validatedStaticPaths.Add((zipTempPath, zipTempRelativePath));
    startupLogger.LogInformation("Registered static path: {Physical} -> /{Virtual}", zipTempPath, zipTempRelativePath);
}

//try
//{
//    Directory.CreateDirectory(zipTempPath);
//    _fileLogger.LogInformation("Ensured zipTemp directory exists: {Path}", zipTempPath);
//}
//catch (System.Exception ex)
//{
//    _fileLogger.LogError(ex, "Failed to ensure zipTemp directory: {Path}", zipTempPath);
//    throw;
//}


var staticPathPlanDocumentFilesConfig = builder.Configuration.GetSection("PlanDocumentFiles").Get<PlanDocumentFiles>();

// Validate configured static paths and log/skips missing ones
if (staticPathPlanDocumentFilesConfig?.Paths != null)
{
    foreach (var path in staticPathPlanDocumentFilesConfig.Paths)
    {
        if (string.IsNullOrWhiteSpace(path.Physical))
        {
            startupLogger.LogWarning("Static path has empty Physical value, skipping (virtual={Virtual}).", path.Virtual);
            continue;
        }

        var physicalPath = Path.IsPathRooted(path.Physical)
            ? path.Physical
            : Path.Combine(builder.Environment.ContentRootPath, path.Physical);

        if (!Directory.Exists(physicalPath))
        {
            startupLogger.LogWarning("Configured static path does not exist, skipping: {Physical} (virtual={Virtual})", physicalPath, path.Virtual);
            continue;
        }

        validatedStaticPaths.Add((physicalPath, path.Virtual ?? string.Empty));
        startupLogger.LogInformation("Registered static path: {Physical} -> /{Virtual}", physicalPath, path.Virtual);
    }
}





// ########## Statiska filer och kataloger
//// Hämta inställningarna till katalog med plandokument via IOptions
//var path = app.Services.GetRequiredService<IOptions<PlanDocumentFiles>>().Value.RootPath;
//// Skapar upp virtuell katalog för att exponera plan-dokument
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(path),
//    RequestPath = "/dokument"
//});
app.UseStaticFiles();
// Lägg till extra kataloger dynamiskt från appsettings.json
// Hämta inställningarna till katalog med plandokument via IOptions
if (validatedStaticPaths.Count > 0)
{
    foreach (var path in validatedStaticPaths)
    {
        // Lägg till varje fysisk katalog med virtuell sökväg
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(path.Physical),
            RequestPath = "/" + path.Virtual.TrimStart('/')
        });
    }
}
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//        zipTempPath),
//    RequestPath = "/zipTemp"
//});



// ########## API-dokumentation med Swagger
app.UseSwagger(conf =>
{
    conf.RouteTemplate = "api/{documentName}/swagger.json";
});
app.UseSwaggerUI(conf =>
{
    //conf.SwaggerEndpoint("/api/docs/v1/swagger.json", "v1");
    //conf.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    conf.RoutePrefix = "api";
});


app.UseRouting();

// ... after app.UseStaticFiles() and before app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();


// Mappa controllers till routes, krävs för att aktivera attributrouting
// Gör att [ApiController]-klasser blir tillgängliga på deras routes.
app.MapControllers();


// för maximal tillförlitlighet i loggning av fel vid omedelbar terminering med en kort synkron fallback
var processLogger = loggerFactory.CreateLogger("Process");
AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    var ex = args.ExceptionObject as Exception;
    processLogger.LogCritical(ex, "Unhandled process-level exception | IsTerminating={IsTerminating}", args.IsTerminating);

    if (args.IsTerminating)
    {
        // Synkron fallback: skriv direkt till loggfil (enkel, robust backup)
        try
        {
            var loggingSettings = app.Services.GetService<Microsoft.Extensions.Options.IOptions<ApplicationSpecificLoggingSettings>>()?.Value;
            var rootPath = loggingSettings?.RootPath ?? app.Environment.ContentRootPath;
            Directory.CreateDirectory(rootPath);
            var fallbackPath = Path.Combine(rootPath, "process_fallback_error.log");
            File.AppendAllText(fallbackPath, $"{DateTime.Now:O} {ex}{Environment.NewLine}");
        }
        catch { /* tyst fel; inget mer att göra */ }
    }
};


app.Run(
    //ctx =>
    //{
    //    pathBaseLogger.LogInformation("PathBase från Context.Request.BasePath: {BasePath}", ctx.Request.PathBase);
    //    // Logga att applikationen startat
    //    var logger = loggerFactory.CreateLogger("Startup");
    //    logger.LogInformation("Plandokument started on {Time}", DateTimeOffset.Now);
    //    return Task.CompletedTask;
    //}
    );
