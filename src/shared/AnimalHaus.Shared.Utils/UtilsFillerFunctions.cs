namespace AnimalHaus.Shared.Utils;

public static class UtilsFillerFunctions
{
    public static string BuildFillerLabel(string systemName) => $"{Normalize(systemName)}-{Suffix()}";

    public static string Normalize(string systemName) => systemName.Trim().ToLowerInvariant();

    public static string Suffix() => "filler";
}
