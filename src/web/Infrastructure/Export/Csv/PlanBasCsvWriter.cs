using Plandokument.Application.Interfaces.Exporters;
using Plandokument.Domain.Entities;
using Plandokument.Infrastructure.Cache;

namespace Plandokument.Infrastructure.Export.Csv;

public sealed class PlanBasCsvWriter
    : CacheCsvWriter<PlanBas>
{
    protected override async Task WriteTypedAsync(
        IEnumerable<PlanBas> data,
        TextWriter writer,
        CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("plan_id;lmakt;egn_akt;uuid;planfk;plannamn;status;status_text;isgenomf;dat_beslut;dat_genomf_f;dat_genomf_t;komdkod;lmakt;local_id_list;rk_gmlid_list;planavgift;akt_pb");

        foreach (var item in data)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await writer.WriteLineAsync(string.Join(";",
                item.Register.plan_id,
                item.Register.lmakt,
                item.Register.egn_akt,
                item.Register.uuid,
                item.Register.planfk,
                item.Register.plannamn,
                item.Register.status,
                item.Register.status_text,
                item.Register.isgenomf.ToString(),
                item.Register.dat_beslut,
                item.Register.dat_genomf_f,
                item.Register.dat_genomf_t,
                item.Register.komdkod,
                item.Geometri.lmakt,
                item.Geometri.local_id_list,
                item.Geometri.rk_gmlid_list,
                item.Geometri.planavgift,
                item.Geometri.akt_pb
                ));
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