using Plandokument.Application.Interfaces.Exporters;
using Plandokument.Domain.Entities;
using Plandokument.Infrastructure.Cache;

namespace Plandokument.Infrastructure.Export.Csv;

public sealed class PlanBerorFastighetCsvWriter
    : CacheCsvWriter<PlanBerorFastighet>
{
    protected override async Task WriteTypedAsync(
        IEnumerable<PlanBerorFastighet> data,
        TextWriter writer,
        CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("nyckel;nyckel_fastighet;fastighet");

        foreach (var item in data)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await writer.WriteLineAsync(
                $"{item.nyckel};{item.nyckel_fastighet.ToString()};{item.fastighet}");
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