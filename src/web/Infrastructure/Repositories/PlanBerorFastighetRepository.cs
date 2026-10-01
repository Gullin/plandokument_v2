using Microsoft.AspNetCore.Connections;
using Plandokument.Application.Interfaces.Repositories;
using Plandokument.Domain.Entities;
using Plandokument.Infrastructure.Sql;
using System.Diagnostics;

namespace Plandokument.Infrastructure.Repositories;

public class PlanBerorFastighetRepository : IPlanBerorFastighetRepository
{
    private readonly ISqlServerConnectionFactory _connectionFactory;
    private readonly ISqlLoader _sqlLoader;
    private readonly ILogger<PlanBerorFastighetRepository> _logger;

    public PlanBerorFastighetRepository(ISqlServerConnectionFactory connectionFactory, ISqlLoader sqlLoader, ILogger<PlanBerorFastighetRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _sqlLoader = sqlLoader;
        _logger = logger;
    }

    public async Task<IEnumerable<PlanBerorFastighet>> GetPlanBerorFastighetAsync(CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();

        var sw = Stopwatch.StartNew();

        try
        {
            await conn.OpenAsync(cancellationToken);

            _logger.LogDebug("Opened DB connection for provider, executing query {QueryName}", "SelectPlanBerorFastighet");

            var sql = _sqlLoader.Load(SqlQuery.SelectPlanBerorFastighet, "mssqlserver");
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var list = new List<PlanBerorFastighet>();
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(new PlanBerorFastighet
                {
                    nyckel = reader.GetString(0),
                    nyckel_fastighet= reader.GetInt32(1),
                    fastighet = reader.GetString(2),
                });
            }

            _logger.LogInformation("Query SelectPlanBerorFastighet returned {Count} rows in {Elapsed}ms", list.Count, sw.ElapsedMilliseconds);

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SelectPlanBerorFastighet");
            throw;
        }
        finally
        {
            sw.Stop();
            await conn.CloseAsync();
            _logger.LogInformation("Executed SelectPlanBerorFastighet in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
        }
    }
}
