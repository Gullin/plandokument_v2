namespace Plandokument.Infrastructure.Sql;

public interface ISqlLoader
{
    string Load(SqlQuery query, string provider);
}

public class SqlLoader : ISqlLoader
{
    private readonly string _basePath;
    private readonly Dictionary<string, string> _cache = new();

    public SqlLoader(string basePath)
    {
        _basePath = basePath;
    }

    public string Load(SqlQuery query, string provider)
    {
        var fileName = query.ToFileName();
        var key = $"{provider}:{fileName}";

        if (_cache.TryGetValue(key, out var sql))
            return sql;

        var path = Path.Combine(_basePath, provider, fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"SQL file not found: {path}");

        sql = File.ReadAllText(path);
        _cache[key] = sql;
        return sql;
    }
}
