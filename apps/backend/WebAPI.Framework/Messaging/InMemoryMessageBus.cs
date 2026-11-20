using System.Threading.Channels;
using WebAPI.Common.Messaging;

namespace WebAPI.Framework.Messaging;

/// <summary>
/// Fila em memória baseada em Channel&lt;T&gt;. Funciona dentro de um único processo.
/// Para WebAPI + Worker em processos separados, troque por RabbitMQ/Service Bus
/// (basta outra implementação de IMessagePublisher + IMessageQueueReader).
/// </summary>
public sealed class InMemoryMessageBus : IMessagePublisher, IMessageQueueReader
{
    private readonly Channel<MessageEnvelope> _channel =
        Channel.CreateUnbounded<MessageEnvelope>(new UnboundedChannelOptions { SingleReader = true });

    public Task PublishAsync(MessageEnvelope envelope, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(envelope, cancellationToken).AsTask();

    public IAsyncEnumerable<MessageEnvelope> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
