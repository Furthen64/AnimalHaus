namespace AnimalHaus.Integration.Tests;

public static class IntegrationTestFillerFunctions
{
    public static string BuildRunId(string systemName, int tick) => $"{NormalizeSystem(systemName)}-{NormalizeTick(tick)}";

    public static string NormalizeSystem(string systemName) => systemName.Trim().ToLowerInvariant();

    public static int NormalizeTick(int tick) => Math.Max(0, tick);
}
