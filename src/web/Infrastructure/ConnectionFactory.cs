using MicrosoftLogg = Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Plandokument.Application.Options.Databases;
using System.Data.Common;

namespace Plandokument.Infrastructure;

public interface ISqlServerConnectionFactory { DbConnection CreateConnection(); }
public interface IPostgresConnectionFactory { DbConnection CreateConnection(); }
public interface ISqliteConnectionFactory { DbConnection CreateConnection(); }


public class ConnectionFactory :
    ISqlServerConnectionFactory,
    IPostgresConnectionFactory,
    ISqliteConnectionFactory
{
    private readonly DatabasesSettings _options;

    public ConnectionFactory(IOptions<DatabasesSettings> options)
    {
        _options = options.Value;
    }

    private DbConnection Create(DatabaseOptions options)
    {
        try
        {
            var factory = DbProviderFactories.GetFactory(options.Provider);
            var conn = factory.CreateConnection()!;
            conn.ConnectionString = options.ConnectionString;
            return conn;

        }
        catch (Exception)
        {
            throw;
        }
    }


    DbConnection ISqlServerConnectionFactory.CreateConnection() => Create(_options.SqlServer);
    DbConnection IPostgresConnectionFactory.CreateConnection() => Create(_options.Postgres);
    DbConnection ISqliteConnectionFactory.CreateConnection() => Create(_options.Sqlite);
}