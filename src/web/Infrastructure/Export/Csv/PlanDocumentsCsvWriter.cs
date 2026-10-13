using Plandokument.Application.Interfaces.Exporters;
using Plandokument.Domain.Entities;
using Plandokument.Infrastructure.Cache;
using System.Globalization;

namespace Plandokument.Infrastructure.Export.Csv;

public sealed class PlanDocumentsCsvWriter
    : CacheCsvWriter<PlanDocuments>
{
    protected override async Task WriteTypedAsync(
        IEnumerable<PlanDocuments> data,
        TextWriter writer,
        CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync(
            "RootPath;VirtualPath;FileName;Extension;FullPath;LengthBytes");

        foreach (var item in data)
        {
            if (item.Documents is null)
                continue;

            foreach (var file in item.Documents)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await writer.WriteLineAsync(string.Join(";",
                    item.RootPath,
                    item.VirtualPath,
                    file.Name,
                    file.Extension,
                    file.FullName,
                    file.Length.ToString()
                    ));
            }
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