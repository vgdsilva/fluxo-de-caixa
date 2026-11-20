using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebAPI.Common.Converters;
using WebAPI.Common.Webhooks;
using WebAPI.Data;
using WebAPI.Data.Entities;

namespace WebAPI.Core.Webhooks;

/// <summary>Cria um WebhookDelivery pendente para cada assinatura ativa que aceita o evento.</summary>
public sealed class WebhookDispatcher(WebAPIContext db) : IWebhookDispatcher
{
    public async Task DispatchAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        var subscriptions = await db.WebhookSubscriptions
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        var payload = JsonSerializer.Serialize(new
        {
            id = webhookEvent.Id,
            type = webhookEvent.EventType,
            occurredAt = webhookEvent.OccurredAt,
            data = webhookEvent.Data,
        }, JsonDefaults.Options);

        foreach (var subscription in subscriptions.Where(s => s.Accepts(webhookEvent.EventType)))
        {
            db.WebhookDeliveries.Add(new WebhookDelivery
            {
                SubscriptionId = subscription.Id,
                EventId = webhookEvent.Id,
                EventType = webhookEvent.EventType,
                Payload = payload,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
