using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using WebAPI.Common.Converters;
using WebAPI.Common.Messaging;

namespace WebAPI.Framework.Messaging;

/// <summary>Desserializa o envelope e chama todos os IMessageHandler&lt;T&gt; registrados.</summary>
public sealed class MessageDispatcher(MessageTypeRegistry registry)
{
    public async Task<bool> DispatchAsync(IServiceProvider services, MessageEnvelope envelope, CancellationToken cancellationToken)
    {
        if (!registry.TryResolve(envelope.Type, out var messageType))
        {
            return false;
        }

        var message = JsonSerializer.Deserialize(envelope.Payload, messageType, JsonDefaults.Options)
            ?? throw new InvalidOperationException($"Payload vazio para a mensagem {envelope.Type}.");

        var invokerType = typeof(HandlerInvoker<>).MakeGenericType(messageType);
        var invoker = (IHandlerInvoker)Activator.CreateInstance(invokerType)!;
        var context = new MessageContext(envelope.Id, envelope.OccurredAt, envelope.CorrelationId);
        await invoker.InvokeAsync(services, message, context, cancellationToken);
        return true;
    }

    private interface IHandlerInvoker
    {
        Task InvokeAsync(IServiceProvider services, object message, MessageContext context, CancellationToken cancellationToken);
    }

    private sealed class HandlerInvoker<TMessage> : IHandlerInvoker where TMessage : IMessage
    {
        public async Task InvokeAsync(IServiceProvider services, object message, MessageContext context, CancellationToken cancellationToken)
        {
            foreach (var handler in services.GetServices<IMessageHandler<TMessage>>())
            {
                await handler.HandleAsync((TMessage)message, context, cancellationToken);
            }
        }
    }
}
