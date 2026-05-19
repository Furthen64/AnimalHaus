using AnimalHaus.Barn;
using AnimalHaus.Shared.Utils;

namespace AnimalHaus.Barn.Modules;

public sealed class QualityControlModule
{
    private readonly int _freshnessDecayMin;
    private readonly int _freshnessDecayMax;

    public int FreshnessScore { get; private set; }

    public QualityControlModule(BarnOptions options)
    {
        FreshnessScore = options.InitialFreshnessScore;
        _freshnessDecayMin = options.FreshnessDecayMin;
        _freshnessDecayMax = options.FreshnessDecayMax;
    }

    public void AdvanceTick(DeterministicRandomProvider randomProvider)
    {
        FreshnessScore = Math.Clamp(FreshnessScore - randomProvider.Next(_freshnessDecayMin, _freshnessDecayMax + 1), 0, 100);
    }
}
