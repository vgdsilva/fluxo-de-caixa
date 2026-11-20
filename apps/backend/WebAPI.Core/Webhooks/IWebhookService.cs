namespace WebAPI.Core.Webhooks;

/// <summary>Cadastro e gerenciamento de assinaturas de webhook (outbound).</summary>
public interface IWebhookService
{
    Task<CreatedWebhookSubscriptionResponse> SubscribeAsync(CreateWebhookSubscriptionRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WebhookSubscriptionResponse>> ListAsync(CancellationToken cancellationToken = default);

    Task UnsubscribeAsync(Guid id, CancellationToken cancellationToken = default);
}
