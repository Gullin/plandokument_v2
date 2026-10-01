using Microsoft.Data.Sqlite;
using NuGet.Protocol.Plugins;
using Plandokument.Application.Options.Logging;
using Plandokument.Infrastructure.Sql;
using System.Collections.Concurrent;

namespace Plandokument.Infrastructure.Logging;

internal class DbStatisticLogger : ILogger, IDisposable
{
    private readonly string _categoryName;
    private readonly ApplicationSpecificLoggingSettings _options;
    private readonly ISqliteConnectionFactory _connectionFactory;
    private readonly ISqlLoader _sqlLoader;
    private readonly HashSet<LogLevel> _allowedLevels;
    //private readonly ILogger _logger;
    private readonly Task _workerTask;
    private readonly BlockingCollection<(string? when, string? nbr_search, string? nbr_hits, string? searchtime)> _queue = new();
    private readonly CancellationTokenSource _cts = new();

    public DbStatisticLogger(string categoryName,
        ApplicationSpecificLoggingSettings options,
        ISqliteConnectionFactory connectionFactory,
        ISqlLoader sqlLoader,
        HashSet<LogLevel> allowedLevels//,
        //ILogger logger
        )
    {
        _categoryName = categoryName;
        _options = options;
        _connectionFactory = connectionFactory;
        _sqlLoader = sqlLoader;
        _allowedLevels = allowedLevels;
        //_logger = logger;

        // Startar bakgrundsarbetsuppgift
        _workerTask = Task.Run(ProcessQueueAsync);
    }

    IDisposable? ILogger.BeginScope<TState>(TState state) => null;

    //public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
    public bool IsEnabled(LogLevel logLevel) => _allowedLevels.Contains(logLevel);

    void ILogger.Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        // Extrahera eventuella parametrar från state
        if (state is IEnumerable<KeyValuePair<string, object>> properties)
        {
            _queue.Add((
                properties.FirstOrDefault(p => p.Key == "when").Value.ToString(),
                properties.FirstOrDefault(p => p.Key == "nbr_search").Value.ToString(),
                properties.FirstOrDefault(p => p.Key == "nbr_hits").Value.ToString(),
                properties.FirstOrDefault(p => p.Key == "searchtime").Value.ToString()
                ));

        }
        else
            return;


    }

    private async Task ProcessQueueAsync()
    {
        if (_categoryName == "RequestStatistics")
        {
            if (!_options.StatisticsRequest.Enabled)
                return;

            foreach (var (when, nbr_search, nbr_hits, searchtime) in _queue.GetConsumingEnumerable(_cts.Token))
            {
                try
                {
                    await WriteToDatabaseAsync(when, nbr_search, nbr_hits, searchtime);
                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex, "Fel vid loggning av statistik: {Message}", ex.Message);
                    // Undertryck loggfel för att inte krascha applikationen
                }
            }
        }
    }

    private async Task WriteToDatabaseAsync(string? when, string? nbr_search, string? nbr_hits, string? searchtime)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync(_cts.Token);


        using (var cmd = conn.CreateCommand())
        {
            //using var transaction = conn.BeginTransaction();
            //cmd.Transaction = transaction;

            var sql = _sqlLoader.Load(SqlQuery.InsertStatRequests, "sqlite");
            cmd.CommandText = sql;
            cmd.Parameters.Add(new SqliteParameter("@when", when ?? DBNull.Value.ToString()));
            cmd.Parameters.Add(new SqliteParameter("@nbr_search", nbr_search ?? DBNull.Value.ToString()));
            cmd.Parameters.Add(new SqliteParameter("@nbr_hits", nbr_hits ?? DBNull.Value.ToString()));
            cmd.Parameters.Add(new SqliteParameter("@searchtime", searchtime ?? DBNull.Value.ToString()));

            // Infogar loggpost
            await cmd.ExecuteNonQueryAsync(_cts.Token);

            //await transaction.CommitAsync(_cts.Token);
        }
    }

    void IDisposable.Dispose()
    {
        _queue.CompleteAdding();
        _cts.Cancel();
        try { _workerTask.Wait(2000); } catch { }
    }
}