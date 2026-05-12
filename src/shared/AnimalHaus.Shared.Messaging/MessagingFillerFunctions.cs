namespace AnimalHaus.Shared.Messaging;

public static class MessagingFillerFunctions
{
    public static string BuildFillerTopic(string systemName) => ComposeTopicSegment(systemName, TopicType());

    public static string ComposeTopicSegment(string systemName, string topicType) => $"{NormalizeSegment(systemName)}.{NormalizeSegment(topicType)}.filler";

    public static string TopicType() => "events";

    public static string NormalizeSegment(string segment) => segment.Trim().ToLowerInvariant();
}
