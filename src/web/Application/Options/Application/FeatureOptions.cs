using Plandokument.Features.FeatureManagement.Enums;

namespace Plandokument.Application.Options.Application;

public sealed class FeatureOptions
{
    public Dictionary<string, FeatureLifecycle> States { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
