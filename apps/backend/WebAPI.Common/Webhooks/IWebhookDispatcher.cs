namespace WebAPI.Common.Webhooks;

/// <summary>
/// Enfileira um evento para todos os assinantes ativos daquele tipo.
/// A entrega real (HTTP, retry, backoff) é feita pelo WebhookDeliveryService.
/// </summary>
public interface IWebhookDispatcher
{
    Task DispatchAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default);
}
