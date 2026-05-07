using AnimalHaus.Shared.Utils;

namespace AnimalHaus.Pigpen.Modules;

public sealed class PenEnvironmentModule
{
    public int CleanlinessScore { get; private set; } = 88;

    public int TemperatureCelsius { get; private set; } = 21;

    public int Density { get; } = 1;

    public void AdvanceTick(DeterministicRandomProvider randomProvider)
    {
        CleanlinessScore = Math.Clamp(CleanlinessScore - randomProvider.Next(1, 4), 0, 100);
        TemperatureCelsius += randomProvider.Next(-1, 2);
    }
}
