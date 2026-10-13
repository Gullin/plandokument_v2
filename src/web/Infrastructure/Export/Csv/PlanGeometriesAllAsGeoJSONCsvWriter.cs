using Plandokument.Application.Interfaces.Exporters;
using Plandokument.Domain.Entities;
using Plandokument.Infrastructure.Cache;

namespace Plandokument.Infrastructure.Export.Csv;

public sealed class PlanGeometriesAllAsGeoJSONCsvWriter
    : CacheCsvWriter<PlanGeoJSON>
{
    protected override async Task WriteTypedAsync(
        IEnumerable<PlanGeoJSON> data,
        TextWriter writer,
        CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("GeoJSON");

        foreach (var item in data)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await writer.WriteLineAsync(
                $"{item.GeoJSON}");
        }
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(';') ||
            value.Contains('"') ||
            value.Contains('\r') ||
            value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}