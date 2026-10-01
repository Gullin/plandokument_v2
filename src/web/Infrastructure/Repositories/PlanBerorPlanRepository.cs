using Microsoft.AspNetCore.Connections;
using Plandokument.Application.Interfaces.Repositories;
using Plandokument.Domain.Entities;
using Plandokument.Infrastructure.Sql;
using System.Diagnostics;

namespace Plandokument.Infrastructure.Repositories;

public class PlanBerorPlanRepository : IPlanBerorPlanRepository
{
    private readonly ISqlServerConnectionFactory _connectionFactory;
    private readonly ISqlLoader _sqlLoader;
    private readonly ILogger<PlanBerorPlanRepository> _logger;

    public PlanBerorPlanRepository(ISqlServerConnectionFactory connectionFactory, ISqlLoader sqlLoader, ILogger<PlanBerorPlanRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _sqlLoader = sqlLoader;
        _logger = logger;
    }

    public async Task<IEnumerable<PlanBerorPlan>> GetPlanBerorPlanAsync(CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        
        var sw = Stopwatch.StartNew();

        try
        {
            await conn.OpenAsync(cancellationToken);

            _logger.LogDebug("Opened DB connection for provider, executing query {QueryName}", "SelectPlanBerorPlan");

            var sql = _sqlLoader.Load(SqlQuery.SelectPlanBerorPlan, "mssqlserver");
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var list = new List<PlanBerorPlan>();
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(new PlanBerorPlan
                {
                    nyckel = reader.GetString(0),
                    planfk= reader.GetString(1),
                    beskrivning = reader.GetString(2),
                    nyckel_pavarkan = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    pavarkan = reader.IsDBNull(4) ? null : reader.GetString(4),
                    pav_planfk = reader.IsDBNull(5) ? null : reader.GetString(5),
                    status_pavarkan = reader.IsDBNull(6) ? null : reader.GetString(6),
                    registrerat_beslut = reader.GetInt32(7),
                });
            }

            _logger.LogInformation("Query SelectPlanBerorPlan returned {Count} rows in {Elapsed}ms", list.Count, sw.ElapsedMilliseconds);

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SelectPlanBerorPlan");
            throw;
        }
        finally
        {
            sw.Stop();
            await conn.CloseAsync();
            _logger.LogInformation("Executed SelectPlanBerorPlan in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
        }
    }
}
