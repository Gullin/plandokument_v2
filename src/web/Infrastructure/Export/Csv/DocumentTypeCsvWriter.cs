using Plandokument.Application.Interfaces.Exporters;
using Plandokument.Domain.Entities;
using Plandokument.Infrastructure.Cache;

namespace Plandokument.Infrastructure.Export.Csv;

public sealed class DocumentTypeCsvWriter
    : CacheCsvWriter<DocumentType>
{
    protected override async Task WriteTypedAsync(
        IEnumerable<DocumentType> data,
        TextWriter writer,
        CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("dokumenttyp;url-filter;suffix;beskrivning;planhandling");

        foreach (var item in data)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await writer.WriteLineAsync(
                $"{item.Type};{Escape(item.UrlFilter)};{Escape(item.Suffix)};{Escape(item.Description)};{item.IsPlanhandling.ToString()}");
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