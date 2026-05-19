using AnimalHaus.Pigpen;

namespace AnimalHaus.Pigpen.Modules;

public sealed class PigLifecycleModule
{
    private readonly int _weightGainFed;
    private readonly int _weightGainUnfed;
    private readonly int _transferWeightThreshold;

    public int AgeInTicks { get; private set; }

    public int Weight { get; private set; }

    public bool IsReadyForTransfer => Weight >= _transferWeightThreshold;

    public PigLifecycleModule(PigpenOptions options)
    {
        AgeInTicks = options.InitialAgeTicks;
        Weight = options.InitialWeight;
        _weightGainFed = options.WeightGainFed;
        _weightGainUnfed = options.WeightGainUnfed;
        _transferWeightThreshold = options.TransferWeightThreshold;
    }

    public void AdvanceTick(bool wasFed)
    {
        AgeInTicks++;
        Weight += wasFed ? _weightGainFed : _weightGainUnfed;
    }
}
