namespace WebAPI.Common.Messaging;

/// <summary>
/// Publica um envelope na fila (InMemory hoje; RabbitMQ/Service Bus depois).
/// Código de negócio normalmente NÃO chama isto direto: usa <see cref="IOutbox"/>,
/// e o OutboxDispatcherService publica de forma confiável.
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync(MessageEnvelope envelope, CancellationToken cancellationToken = default);
}
