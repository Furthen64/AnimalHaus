using AnimalHaus.Shared.Utils;
using NetMQ;
using NetMQ.Sockets;

namespace AnimalHaus.Shared.Messaging;

public sealed class NetMqCommandServer : IDisposable
{
    private readonly ResponseSocket socket = new();

    public NetMqCommandServer(string bindEndpoint)
    {
        socket.Options.Linger = TimeSpan.Zero;
        socket.Bind(bindEndpoint);
    }

    public bool TryReceive(out MessageEnvelope envelope)
    {
        envelope = default!;
        if (!socket.TryReceiveFrameString(TimeSpan.FromMilliseconds(10), out var requestJson) || string.IsNullOrWhiteSpace(requestJson))
        {
            return false;
        }

        envelope = JsonMessageSerializer.Deserialize<MessageEnvelope>(requestJson);
        return true;
    }

    public void Reply<T>(string topic, T response, string correlationId, string? causationId = null)
    {
        var envelope = EnvelopeFactory.Create(topic, response, correlationId, causationId);
        socket.SendFrame(JsonMessageSerializer.Serialize(envelope));
    }

    public void Dispose() => socket.Dispose();
}
