using AnimalHaus.Pigpen;
using AnimalHaus.Shared.Utils;

namespace AnimalHaus.Pigpen.Modules;

public sealed class PenEnvironmentModule
{
    private readonly int _cleanlinessDecayMin;
    private readonly int _cleanlinessDecayMax;
    private readonly int _temperatureDeltaMin;
    private readonly int _temperatureDeltaMax;

    public int CleanlinessScore { get; private set; }

    public int TemperatureCelsius { get; private set; }

    public int Density { get; } = 1;

    public PenEnvironmentModule(PigpenOptions options)
    {
        CleanlinessScore = options.InitialCleanliness;
        TemperatureCelsius = options.InitialTemperature;
        _cleanlinessDecayMin = options.CleanlinessDecayMin;
        _cleanlinessDecayMax = options.CleanlinessDecayMax;
        _temperatureDeltaMin = options.TemperatureDeltaMin;
        _temperatureDeltaMax = options.TemperatureDeltaMax;
    }

    public void AdvanceTick(DeterministicRandomProvider randomProvider)
    {
        CleanlinessScore = Math.Clamp(CleanlinessScore - randomProvider.Next(_cleanlinessDecayMin, _cleanlinessDecayMax + 1), 0, 100);
        TemperatureCelsius += randomProvider.Next(_temperatureDeltaMin, _temperatureDeltaMax + 1);
    }
}
