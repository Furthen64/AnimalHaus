namespace AnimalHaus.Shared.Utils;

public sealed class DeterministicRandomProvider
{
    private readonly Random random;

    public DeterministicRandomProvider(int seed)
    {
        random = new Random(seed);
    }

    public int Next(int minValue, int maxValue) => random.Next(minValue, maxValue);

    public double NextDouble() => random.NextDouble();
}
