using System.Text.RegularExpressions;

public sealed class ConsoleSqlTableExtractor
{
    private static readonly Regex FromJoinRegex = new(
        @"\b(?:FROM|JOIN)\s+([^\s,()]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public IReadOnlyList<SqlTableUsage> Tables { get; }

    public IReadOnlyList<string> UniqueTables { get; }

    private static readonly HashSet<string> SqlFileExtensions =
    [
        ".sql",
        ".pgsql"
    ];

    public ConsoleSqlTableExtractor(string relativeSqlDirectory = "Infrastructure/Sql")
    {
        var sqlDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            relativeSqlDirectory);

        if (!Directory.Exists(sqlDirectory))
        {
            Tables = [];
            UniqueTables = [];

            Console.WriteLine($"Katalogen finns inte: {sqlDirectory}");
            return;
        }

        Tables = ReadTables(sqlDirectory);

        UniqueTables = Tables
            .Select(x => x.Table)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<SqlTableUsage> ReadTables(string sqlDirectory)
    {
        var result = new List<SqlTableUsage>();

        foreach (var file in Directory.EnumerateFiles(
            sqlDirectory,
            "*.*",
            SearchOption.AllDirectories)
            .Where(file =>
                SqlFileExtensions.Contains(
                    Path.GetExtension(file),
                    StringComparer.OrdinalIgnoreCase)))
        {
            var sql = File.ReadAllText(file);

            // Ta bort kommenterade rader.
            sql = RemoveCommentLines(sql);

            foreach (Match match in FromJoinRegex.Matches(sql))
            {
                result.Add(new SqlTableUsage(
                    Path.GetRelativePath(sqlDirectory, file),
                    match.Groups[1].Value));
            }
        }

        return result;
    }

    private static string RemoveCommentLines(string sql)
    {
        return string.Join(
            Environment.NewLine,
            sql.Split(
                ["\r\n", "\n", "\r"],
                StringSplitOptions.None)
            .Where(line => !line.TrimStart().StartsWith("--")));
    }

    public void Print()
    {
        Console.WriteLine("=== Tabeller/vyer per SQL-fil ===");
        Console.WriteLine();

        foreach (var usage in Tables)
        {
            Console.WriteLine($"{usage.File,-60} {usage.Table}");
        }

        Console.WriteLine();
        Console.WriteLine("=== Unika tabeller/vyer ===");
        Console.WriteLine();

        foreach (var table in UniqueTables)
        {
            Console.WriteLine(table);
        }

        Console.WriteLine();
        Console.WriteLine($"Antal träffar : {Tables.Count}");
        Console.WriteLine($"Antal unika   : {UniqueTables.Count}");
    }

    public static void Main(string[] args)
    {
        //var sqlDirectory = args.Length > 0
        //    ? args[0]
        //    : Path.Combine(
        //        Directory.GetCurrentDirectory(),
        //        "Infrastructure",
        //        "Sql");

        var sqlDirectory = args.Length > 0
            ? args[0]
            : "";
        var extractor = new ConsoleSqlTableExtractor(sqlDirectory);

        extractor.Print();
    }
}

public sealed record SqlTableUsage(
    string File,
    string Table);