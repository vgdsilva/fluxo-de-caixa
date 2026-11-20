namespace WebAPI.Common.Webhooks;

/// <summary>Evento a ser enviado (outbound) para os assinantes cadastrados.</summary>
public sealed record WebhookEvent(
    Guid Id,
    string EventType,
    object Data,
    DateTimeOffset OccurredAt)
{
    public static WebhookEvent Create(string eventType, object data) =>
        new(Guid.NewGuid(), eventType, data, DateTimeOffset.UtcNow);
}
