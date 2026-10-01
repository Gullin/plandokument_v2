namespace Plandokument.Application.Options.Databases;

public class DatabasesSettings
{
    public DatabaseOptions SqlServer { get; set; } = new();
    public DatabaseOptions Postgres { get; set; } = new();
    public DatabaseOptions Sqlite { get; set; } = new();
}