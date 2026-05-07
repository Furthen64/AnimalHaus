using AnimalHaus.Shared.Utils;
using NetMQ;
using NetMQ.Sockets;

namespace AnimalHaus.Shared.Messaging;

public sealed class NetMqSubscriber : IDisposable
{
    private readonly SubscriberSocket socket = new();

    public NetMqSubscriber(IEnumerable<string> connectEndpoints, IEnumerable<string> topics)
    {
        socket.Options.Linger = TimeSpan.Zero;
        foreach (var endpoint in connectEndpoints.Where(static endpoint => !string.IsNullOrWhiteSpace(endpoint)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            socket.Connect(endpoint);
        }

        foreach (var topic in topics.Where(static topic => !string.IsNullOrWhiteSpace(topic)).Distinct(StringComparer.Ordinal))
        {
            socket.Subscribe(topic);
        }
    }

    public bool TryReceive(out MessageEnvelope envelope)
    {
        envelope = default!;
        List<string>? frames = null;
        if (!socket.TryReceiveMultipartStrings(TimeSpan.FromMilliseconds(10), ref frames, 2) || frames is null)
        {
            return false;
        }

        envelope = JsonMessageSerializer.Deserialize<MessageEnvelope>(frames[1]);
        return true;
    }

    public void Dispose() => socket.Dispose();
}
