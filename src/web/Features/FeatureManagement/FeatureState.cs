using Plandokument.Domain.Interfaces;

namespace Plandokument.Features.FeatureManagement.Features;

public sealed class FeatureState : IFeatureState
{
    public bool AdminMenyEnabled { get; init; }
}
