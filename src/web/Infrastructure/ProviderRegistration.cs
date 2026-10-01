using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Npgsql;
using System.Data.Common;

namespace Plandokument.Infrastructure;

public static class ProviderRegistration
{
    public static void Register()
    {
        // SQL Server
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);

        // PostgreSQL
        DbProviderFactories.RegisterFactory("Npgsql", NpgsqlFactory.Instance);

        // SQLite
        DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", SqliteFactory.Instance);
    }
}
