namespace WebAPI.Common.Webhooks;

public static class WebhookHeaders
{
    public const string Signature = "X-Webhook-Signature";
    public const string EventType = "X-Webhook-Event";
    public const string DeliveryId = "X-Webhook-Delivery";
}
