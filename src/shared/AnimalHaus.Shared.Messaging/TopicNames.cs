namespace AnimalHaus.Shared.Messaging;

public static class TopicNames
{
    public const string SchemaVersion = "1.0";

    public static string Event(string systemName, string eventName) => $"{systemName.ToLowerInvariant()}.events.{eventName}";

    public static string Command(string systemName, string commandName) => $"{systemName.ToLowerInvariant()}.commands.{commandName}";
}
