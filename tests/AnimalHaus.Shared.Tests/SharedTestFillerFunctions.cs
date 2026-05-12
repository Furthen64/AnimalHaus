namespace AnimalHaus.Shared.Tests;

public static class SharedTestFillerFunctions
{
    public static string BuildScenarioName(string baseName) => $"{NormalizeName(baseName)}-{ScenarioSuffix()}";

    public static string NormalizeName(string baseName) => baseName.Trim().ToLowerInvariant();

    public static string ScenarioSuffix() => "shared";
}
