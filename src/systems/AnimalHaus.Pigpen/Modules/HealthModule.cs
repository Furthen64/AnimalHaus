namespace AnimalHaus.Pigpen.Modules;

public sealed class HealthModule
{
    public int HealthScore { get; private set; } = 80;

    public bool Update(int hungerScore, int cleanlinessScore)
    {
        var previous = HealthScore;
        HealthScore = Math.Clamp(100 - (hungerScore / 2) + (cleanlinessScore / 4), 0, 100);
        return previous != HealthScore;
    }
}
