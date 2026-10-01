using Microsoft.AspNetCore.Connections;
using Plandokument.Application.Interfaces.Repositories;
using Plandokument.Application.Interfaces.Services;
using Plandokument.Domain.Entities;
using Plandokument.Infrastructure.Sql;
using System.Diagnostics;

namespace Plandokument.Infrastructure.Repositories;

public class PlanGeometriesBySearchAsGeoJSONRepository : IPlanGeometryBySearchAsGeoJSONRepository
{
    private readonly IPostgresConnectionFactory _connectionFactory;
    private readonly ISqlLoader _sqlLoader;
    private readonly IPlanBasService _planBasService;
    private readonly ILogger<PlanGeometriesBySearchAsGeoJSONRepository> _logger;

    public PlanGeometriesBySearchAsGeoJSONRepository(IPostgresConnectionFactory connectionFactory, ISqlLoader sqlLoader, IPlanBasService planBasService, ILogger<PlanGeometriesBySearchAsGeoJSONRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _sqlLoader = sqlLoader;
        _planBasService = planBasService;
        _logger = logger;
    }

    public async Task<PlanGeoJSON> GetPlanGeometryBySearchAsGeoJSONAsync(List<string> planIds, CancellationToken cancellationToken = default)
    {
        var allPlanBas = await _planBasService.GetAllAsync();
        var lmaktList = allPlanBas
            .Where(pb => planIds.Contains(pb.Register.plan_id) && !string.IsNullOrEmpty(pb.Register.lmakt))
            .Select(pb => pb.Register.lmakt)
            .ToList();

        string planAktsAsCsv = "'" + string.Join("','", lmaktList) + "'";


        using var conn = _connectionFactory.CreateConnection();

        var sw = Stopwatch.StartNew();

        try
        {
            await conn.OpenAsync(cancellationToken);

            _logger.LogDebug("Opened DB connection for provider, executing query {QueryName}", "SelectPlanGeometriBySearch");

            var sql = _sqlLoader.Load(SqlQuery.SelectPlanGeometriBySearch, "postgresql");
            sql = sql.Replace("@search_string", planAktsAsCsv);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            //cmd.CommandText.Replace("@search_string", planAktsAsCsv);

            var geoJSON = new PlanGeoJSON { GeoJSON = "{}" };
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                geoJSON.GeoJSON = reader.GetString(0);
            }

            _logger.LogInformation("Query SelectPlanGeometriBySearch returned rows in {Elapsed}ms", sw.ElapsedMilliseconds);

            return geoJSON;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SelectPlanGeometriBySearch");
            throw;
        }
        finally
        {
            sw.Stop();
            await conn.CloseAsync();
            _logger.LogInformation("Executed SelectPlanGeometriBySearch in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
        }
    }
}
