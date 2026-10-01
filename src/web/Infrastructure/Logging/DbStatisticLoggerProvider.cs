using Microsoft.AspNetCore.Connections;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Plandokument.Application.Options.Logging;
using Plandokument.Infrastructure.Sql;
using System.Threading;

namespace Plandokument.Infrastructure.Logging;

public class DbStatisticLoggerProvider : ILoggerProvider
{
    private readonly ApplicationSpecificLoggingSettings _options;
    private readonly ISqliteConnectionFactory _connectionFactory;
    private readonly ISqlLoader _sqlLoader;
    private readonly CancellationTokenSource _cts = new();

    private const string TargetCategory = "RequestStatistics";

    // Levels to log
    private static readonly HashSet<LogLevel> AllowedLevels = new()
    {
        LogLevel.Information
    };

    public DbStatisticLoggerProvider(IOptions<ApplicationSpecificLoggingSettings> options,
        ISqliteConnectionFactory connectionFactory,
        ISqlLoader sqlLoader
        )
    {
        _options = options.Value;
        _connectionFactory = connectionFactory;
        _sqlLoader = sqlLoader;

        // Ensure database file and containing directory exist
        var dbPath = Path.Combine(_options.RootPath, "PlandokumentAppDb.sqlite");
        var dbDir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }

        if (!File.Exists(dbPath))
        {
            File.WriteAllBytes(dbPath, Array.Empty<byte>());
        }

        using var conn = _connectionFactory.CreateConnection();
        conn.OpenAsync(_cts.Token);


        var sql = _sqlLoader.Load(SqlQuery.CreateStatRequestsTable, "sqlite");
        using (var cmd = conn.CreateCommand())
        {
            //using var transaction = conn.BeginTransaction();
            //cmd.Transaction = transaction;

            cmd.CommandText = sql;

            // Skapar tabell om den inte finns
            try
            {
                cmd.ExecuteNonQueryAsync(_cts.Token);

                //await transaction.CommitAsync(_cts.Token);
            }
            catch
            {
                throw;
            }
            finally { conn.Close(); }
        }
    }

    //ILogger ILoggerProvider.CreateLogger(string categoryName) => new DbStatisticLogger(categoryName, _options, _connectionFactory, _sqlLoader);
    ILogger ILoggerProvider.CreateLogger(string categoryName)
    {
        if (categoryName == TargetCategory)
        {
            return new DbStatisticLogger(categoryName, _options, _connectionFactory, _sqlLoader, AllowedLevels);
        }
        else
        {
            return NullLogger.Instance;
        }
    }

    void IDisposable.Dispose() { }
}
