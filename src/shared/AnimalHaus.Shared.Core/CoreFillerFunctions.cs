namespace AnimalHaus.Shared.Core;

public static class CoreFillerFunctions
{
    public static int BuildStabilityScore(int seed) => CombineWithBaseline(NormalizeSeed(seed));

    public static int NormalizeSeed(int seed) => Math.Abs(seed % 10);

    public static int CombineWithBaseline(int normalizedSeed) => normalizedSeed + Baseline();

    private static int Baseline() => 3;
}
