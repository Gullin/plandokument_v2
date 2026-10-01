using Microsoft.AspNetCore.Connections;
using Plandokument.Application.Interfaces.Repositories;
using Plandokument.Domain.Entities;
using Plandokument.Infrastructure.Sql;
using System.Diagnostics;

namespace Plandokument.Infrastructure.Repositories;

public class PlanGeometriesAllAsGeoJSONRepository : IPlanGeometriesAllAsGeoJSONRepository
{
    private readonly IPostgresConnectionFactory _connectionFactory;
    private readonly ISqlLoader _sqlLoader;
    private readonly ILogger<PlanGeometriesAllAsGeoJSONRepository> _logger;

    public PlanGeometriesAllAsGeoJSONRepository(IPostgresConnectionFactory connectionFactory, ISqlLoader sqlLoader, ILogger<PlanGeometriesAllAsGeoJSONRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _sqlLoader = sqlLoader;
        _logger = logger;
    }

    public async Task<IEnumerable<PlanGeoJSON>> GetPlanGeometriesAllAsGeoJSONAsync(CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();

        var sw = Stopwatch.StartNew();

        try
        {
            await conn.OpenAsync(cancellationToken);

            _logger.LogDebug("Opened DB connection for provider, executing query {QueryName}", "SelectPlanGeometriAll");

            var sql = _sqlLoader.Load(SqlQuery.SelectPlanGeometriAll, "postgresql");
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var list = new List<PlanGeoJSON>();
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(new PlanGeoJSON
                {
                    GeoJSON = reader.GetString(0)
                });
            }

            _logger.LogInformation("Query SelectPlanGeometriAll returned {Count} rows in {Elapsed}ms", list.Count, sw.ElapsedMilliseconds);

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SelectPlanGeometriAll");
            throw;
        }
        finally
        {
            sw.Stop();
            await conn.CloseAsync();
            _logger.LogInformation("Executed SelectPlanGeometriAll in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
        }
    }
}
