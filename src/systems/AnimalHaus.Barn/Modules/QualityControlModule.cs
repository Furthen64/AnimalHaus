using AnimalHaus.Shared.Utils;

namespace AnimalHaus.Barn.Modules;

public sealed class QualityControlModule
{
    public int FreshnessScore { get; private set; } = 100;

    public void AdvanceTick(DeterministicRandomProvider randomProvider)
    {
        FreshnessScore = Math.Clamp(FreshnessScore - randomProvider.Next(1, 3), 0, 100);
    }
}
