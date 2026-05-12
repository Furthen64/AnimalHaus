namespace AnimalHaus.Shared.Messaging;

public static class MessagingFillerFunctions
{
    public static string BuildFillerTopic(string systemName) => ComposeTopicSegment(systemName, TopicType());

    public static string ComposeTopicSegment(string systemName, string topicType) => $"{systemName.Trim().ToLowerInvariant()}.{topicType.Trim().ToLowerInvariant()}.filler";

    public static string TopicType() => "events";
}
