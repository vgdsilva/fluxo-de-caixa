namespace WebAPI.Common.Messaging;

/// <summary>Um handler por tipo de mensagem (implementações ficam em Core/Messaging/Handlers).</summary>
public interface IMessageHandler<in TMessage> where TMessage : IMessage
{
    Task HandleAsync(TMessage message, MessageContext context, CancellationToken cancellationToken);
}
