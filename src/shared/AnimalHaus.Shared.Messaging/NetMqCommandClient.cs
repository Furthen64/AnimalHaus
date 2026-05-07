using AnimalHaus.Shared.Utils;
using NetMQ;
using NetMQ.Sockets;

namespace AnimalHaus.Shared.Messaging;

public sealed class NetMqCommandClient
{
    public TResponse Send<TRequest, TResponse>(string endpoint, string topic, TRequest request, string correlationId, string? causationId = null, int timeoutMs = 2000)
    {
        return RetryPolicy.Execute(() =>
        {
            using var socket = new RequestSocket();
            socket.Options.Linger = TimeSpan.Zero;
            socket.Connect(endpoint);

            var envelope = EnvelopeFactory.Create(topic, request, correlationId, causationId);
            socket.SendFrame(JsonMessageSerializer.Serialize(envelope));

            if (!socket.TryReceiveFrameString(TimeSpan.FromMilliseconds(timeoutMs), out var responseJson) || string.IsNullOrWhiteSpace(responseJson))
            {
                throw new TimeoutException($"Timed out waiting for response from {endpoint}.");
            }

            var responseEnvelope = JsonMessageSerializer.Deserialize<MessageEnvelope>(responseJson);
            return JsonMessageSerializer.Deserialize<TResponse>(responseEnvelope.PayloadJson);
        });
    }
}
