using System.Text.Json;

namespace AnimalHaus.Shared.Utils;

public static class StructuredLog
{
    public static void Write(string system, string kind, string name, int? tick = null, string? correlationId = null, object? data = null)
    {
        var payload = new
        {
            timestampUtc = DateTime.UtcNow,
            system,
            kind,
            name,
            tick,
            correlationId,
            data,
        };

        Console.WriteLine(JsonSerializer.Serialize(payload, JsonMessageSerializer.DefaultOptions));
    }
}
