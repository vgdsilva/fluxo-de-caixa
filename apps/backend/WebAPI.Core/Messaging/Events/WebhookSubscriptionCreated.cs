using WebAPI.Common.Messaging;

namespace WebAPI.Core.Messaging.Events;

/// <summary>Exemplo de evento de domínio que passa pelo outbox → fila → handler.</summary>
[MessageType("webhooks.subscription-created")]
public sealed record WebhookSubscriptionCreated(Guid SubscriptionId, string Name, IReadOnlyList<string> EventTypes) : IMessage;
