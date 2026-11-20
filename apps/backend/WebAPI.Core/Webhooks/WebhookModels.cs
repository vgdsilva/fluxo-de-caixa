namespace WebAPI.Core.Webhooks;

public sealed record CreateWebhookSubscriptionRequest(string Name, string Url, IReadOnlyList<string> EventTypes, string? Secret);

public sealed record WebhookSubscriptionResponse(
    Guid Id,
    string Name,
    string Url,
    IReadOnlyList<string> EventTypes,
    bool IsActive,
    DateTimeOffset CreatedAt);

/// <summary>Retornado só na criação: é a única vez que o segredo aparece.</summary>
public sealed record CreatedWebhookSubscriptionResponse(WebhookSubscriptionResponse Subscription, string Secret);
