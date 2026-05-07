using AnimalHaus.Shared.Utils;
using NetMQ;
using NetMQ.Sockets;

namespace AnimalHaus.Shared.Messaging;

public sealed class NetMqPublisher : IDisposable
{
    private readonly PublisherSocket socket = new();

    public NetMqPublisher(string bindEndpoint)
    {
        socket.Options.Linger = TimeSpan.Zero;
        socket.Bind(bindEndpoint);
    }

    public void Publish(MessageEnvelope envelope)
    {
        RetryPolicy.Execute(() =>
        {
            socket.SendMoreFrame(envelope.Topic).SendFrame(JsonMessageSerializer.Serialize(envelope));
            return true;
        });
    }

    public void Dispose() => socket.Dispose();
}
