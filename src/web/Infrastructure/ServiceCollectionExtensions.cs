using Plandokument.Application.Interfaces.Caches;
using Plandokument.Application.Interfaces.Repositories;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Application.Options.Databases;
using Plandokument.Application.Services;
using Plandokument.Infrastructure.Cache;
using Plandokument.Infrastructure.Repositories;
using Plandokument.Infrastructure.Sql;

namespace Plandokument.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        ProviderRegistration.Register();

        services.Configure<DatabasesSettings>(configuration.GetSection("Databases"));

        services.AddSingleton<ISqlLoader>(sp =>
            new SqlLoader(Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Sql")));


        // Registrera ConnectionFactory som implementerar alla tre gränssnitten för databasanslutningar
        services.AddSingleton<ConnectionFactory>();
        services.AddSingleton<ISqlServerConnectionFactory>(sp => sp.GetRequiredService<ConnectionFactory>());
        services.AddSingleton<IPostgresConnectionFactory>(sp => sp.GetRequiredService<ConnectionFactory>());
        services.AddSingleton<ISqliteConnectionFactory>(sp => sp.GetRequiredService<ConnectionFactory>());

        services.AddSingleton<IPlanGeometriBasRepository, PlanGeometriBasRepository>();        // PostgreSQL
        services.AddSingleton<IPlanRegisterBasRepository, PlanRegisterBasRepository>();        // SQL Server

        services.AddSingleton<IPlanBasRepository, PlanBasRepository>();
        services.AddSingleton<IPlanBasService, PlanBasService>();
        services.AddSingleton<IPlanBasCacheService, PlanBasCacheService>();

        services.AddSingleton<IPlanBerorPlanRepository, PlanBerorPlanRepository>();
        services.AddSingleton<IPlanBerorPlanService, PlanBerorPlanService>();
        services.AddSingleton<IPlanBerorPlanCacheService, PlanBerorPlanCacheService>();

        services.AddSingleton<IPlanBerorFastighetRepository, PlanBerorFastighetRepository>();
        services.AddSingleton<IPlanBerorFastighetService, PlanBerorFastighetService>();
        services.AddSingleton<IPlanBerorFastighetCacheService, PlanBerorFastighetCacheService>();

        services.AddSingleton<IPlanGeometriesAllAsGeoJSONRepository, PlanGeometriesAllAsGeoJSONRepository>();
        services.AddSingleton<IPlanGeometriesAllAsGeoJSONService, PlanGeometriesAllAsGeoJSONService>();
        services.AddSingleton<IPlanGeometriesAllAsGeoJSONCacheService, PlanGeometriesAllAsGeoJSONCacheService>();

        services.AddSingleton<IPlanGeometryBySearchAsGeoJSONRepository, PlanGeometriesBySearchAsGeoJSONRepository>();

        return services;
    }
}
