namespace AnimalHaus.Pigpen.Modules;

public sealed class PigLifecycleModule
{
    public int AgeInTicks { get; private set; } = 1;

    public int Weight { get; private set; } = 100;

    public bool IsReadyForTransfer => Weight >= 140;

    public void AdvanceTick(bool wasFed)
    {
        AgeInTicks++;
        Weight += wasFed ? 12 : 4;
    }
}
