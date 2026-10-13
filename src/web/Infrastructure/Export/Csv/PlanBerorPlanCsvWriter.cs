using Plandokument.Application.Interfaces.Exporters;
using Plandokument.Domain.Entities;
using Plandokument.Infrastructure.Cache;

namespace Plandokument.Infrastructure.Export.Csv;

public sealed class PlanBerorPlanCsvWriter
    : CacheCsvWriter<PlanBerorPlan>
{
    protected override async Task WriteTypedAsync(
        IEnumerable<PlanBerorPlan> data,
        TextWriter writer,
        CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("nyckel;planfk;beskrivning;nyckel_pavarkan;pavarkan;pav_planfk;status_pavarkan;registrerat_beslut");

        foreach (var item in data)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await writer.WriteLineAsync(
                $"{item.nyckel};{item.planfk};{item.beskrivning};{item.nyckel_pavarkan.ToString()};{item.pavarkan};{item.pav_planfk};{item.status_pavarkan};{item.registrerat_beslut}");
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