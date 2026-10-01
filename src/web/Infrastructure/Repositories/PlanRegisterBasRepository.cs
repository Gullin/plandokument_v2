using Microsoft.AspNetCore.Connections;
using Plandokument.Application.Interfaces.Repositories;
using Plandokument.Domain.Entities;
using Plandokument.Infrastructure.Sql;
using System.Diagnostics;

namespace Plandokument.Infrastructure.Repositories;

public class PlanRegisterBasRepository : IPlanRegisterBasRepository
{
    private readonly ISqlServerConnectionFactory _connectionFactory;
    private readonly ISqlLoader _sqlLoader;
    private readonly ILogger<PlanRegisterBasRepository> _logger;

    public PlanRegisterBasRepository(ISqlServerConnectionFactory connectionFactory, ISqlLoader sqlLoader, ILogger<PlanRegisterBasRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _sqlLoader = sqlLoader;
        _logger = logger;
    }

    public async Task<IEnumerable<PlanRegisterBas>> GetPlanRegisterBasAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();

            var sw = Stopwatch.StartNew();
            try
            {
                await conn.OpenAsync(cancellationToken);

                _logger.LogDebug("Opened DB connection for provider, executing query {QueryName}", "SelectPlanRegisterBas");

                var sql = _sqlLoader.Load(SqlQuery.SelectPlanRegisterBas, "mssqlserver");
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;

                var list = new List<PlanRegisterBas>();
                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    list.Add(new PlanRegisterBas
                    {
                        plan_id = reader.GetString(0),
                        lmakt = reader.IsDBNull(1) ? null : reader.GetString(1),
                        egn_akt = reader.IsDBNull(2) ? null : reader.GetString(2),
                        uuid = reader.GetString(3),
                        planfk = reader.GetString(4),
                        plannamn = reader.IsDBNull(5) ? null : reader.GetString(5),
                        status = reader.GetString(6),
                        status_text = reader.GetString(7),
                        isgenomf = reader.GetInt32(8),
                        dat_beslut = reader.IsDBNull(9) ? null : reader.GetString(9),
                        dat_genomf_f = reader.IsDBNull(10) ? null : reader.GetString(10),
                        dat_genomf_t = reader.IsDBNull(11) ? null : reader.GetString(11),
                        komdkod = reader.IsDBNull(12) ? null : reader.GetString(12)
                    });
                }

                _logger.LogInformation("Query SelectPlanRegisterBas returned {Count} rows in {Elapsed} ms", list.Count, sw.ElapsedMilliseconds);

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SelectPlanRegisterBas");
                throw;
            }
            finally
            {
                sw.Stop();
                await conn.CloseAsync();
                _logger.LogInformation("Executed SelectPlanRegisterBas in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Error CreateConnection");
            throw;
        }
    }
}
