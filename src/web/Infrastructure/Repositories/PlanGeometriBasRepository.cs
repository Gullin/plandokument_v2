using Microsoft.AspNetCore.Connections;
using Plandokument.Application.Interfaces.Repositories;
using Plandokument.Domain.Entities;
using Plandokument.Infrastructure.Sql;
using System.Data;
using System.Diagnostics;

namespace Plandokument.Infrastructure.Repositories;

public class PlanGeometriBasRepository : IPlanGeometriBasRepository
{
    private readonly IPostgresConnectionFactory _connectionFactory;
    private readonly ISqlLoader _sqlLoader;
    private readonly ILogger<PlanGeometriBasRepository> _logger;

    public PlanGeometriBasRepository(IPostgresConnectionFactory connectionFactory, ISqlLoader sqlLoader, ILogger<PlanGeometriBasRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _sqlLoader = sqlLoader;
        _logger = logger;
    }

    public async Task<IEnumerable<PlanGeometriBas>> GetPlanGeometriBasAsync(CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();

        var sw = Stopwatch.StartNew();

        try
        {
            await conn.OpenAsync(cancellationToken);

            _logger.LogDebug("Opened DB connection for provider, executing query {QueryName}", "SelectPlanGeometriBas");

            var sql = _sqlLoader.Load(SqlQuery.SelectPlanGeometriBas, "postgresql");
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;

            var list = new List<PlanGeometriBas>();
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            int fieldCount = reader.FieldCount;
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(new PlanGeometriBas
                {
                    lmakt = reader.GetString(0),
                    local_id_list = reader.GetString(1),
                    rk_gmlid_list = reader.GetString(2),
                    planavgift = reader.IsDBNull(3) ? null : reader.GetString(3),
                    akt_pb = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }

            _logger.LogInformation("Query SelectPlanGeometriBas returned {Count} rows in {Elapsed}ms", list.Count, sw.ElapsedMilliseconds);

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SelectPlanGeometriBas");
            throw;
        }
        finally
        {
            sw.Stop();
            await conn.CloseAsync();
            _logger.LogInformation("Executed SelectPlanGeometriBas in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
        }
    }
}
