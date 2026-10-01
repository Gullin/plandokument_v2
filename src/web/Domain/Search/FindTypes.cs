namespace Plandokument.Domain.Search;

public static class FindTypes
{
    internal static string Exact { get; } = "Exact";
    internal static string IsPart { get; } = "IsPart";
    internal static string Unmanaged { get; } = "Unmanaged";
}
