namespace WebAPI.Data.Entities;

public enum WebhookDeliveryStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2,
}

/// <summary>Uma tentativa (com retries) de entregar um evento a uma assinatura.</summary>
public class WebhookDelivery : EntityBase
{
    public Guid SubscriptionId { get; set; }

    public WebhookSubscription? Subscription { get; set; }

    public Guid EventId { get; set; }

    public required string EventType { get; set; }

    public required string Payload { get; set; }

    public WebhookDeliveryStatus Status { get; set; } = WebhookDeliveryStatus.Pending;

    public int Attempts { get; set; }

    public DateTimeOffset NextAttemptAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? DeliveredAt { get; set; }

    public int? LastStatusCode { get; set; }

    public string? LastError { get; set; }
}
