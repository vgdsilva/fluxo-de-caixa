using Microsoft.Extensions.Logging;
using WebAPI.Common.Messaging;
using WebAPI.Core.Messaging.Events;

namespace WebAPI.Core.Messaging.Handlers;

/// <summary>Um handler por tipo de mensagem. Este só registra em log; serve de modelo.</summary>
public sealed class WebhookSubscriptionCreatedHandler(ILogger<WebhookSubscriptionCreatedHandler> logger)
    : IMessageHandler<WebhookSubscriptionCreated>
{
    public Task HandleAsync(WebhookSubscriptionCreated message, MessageContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Assinatura de webhook {SubscriptionId} ({Name}) criada para {EventTypes}.",
            message.SubscriptionId, message.Name, string.Join(", ", message.EventTypes));
        return Task.CompletedTask;
    }
}
