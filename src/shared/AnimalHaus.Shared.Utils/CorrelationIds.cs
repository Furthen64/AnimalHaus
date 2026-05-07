namespace AnimalHaus.Shared.Utils;

public static class CorrelationIds
{
    public static string New() => Guid.NewGuid().ToString("N");
}
