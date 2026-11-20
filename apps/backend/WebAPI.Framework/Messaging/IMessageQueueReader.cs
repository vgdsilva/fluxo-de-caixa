using WebAPI.Common.Messaging;

namespace WebAPI.Framework.Messaging;

/// <summary>Lado consumidor da fila (usado pelo QueueConsumerService).</summary>
public interface IMessageQueueReader
{
    IAsyncEnumerable<MessageEnvelope> ReadAllAsync(CancellationToken cancellationToken);
}
