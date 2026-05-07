using System.Text.Json;

namespace AnimalHaus.Shared.Utils;

public static class JsonMessageSerializer
{
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, DefaultOptions);

    public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, DefaultOptions)
        ?? throw new InvalidOperationException($"Unable to deserialize {typeof(T).Name}.");
}
