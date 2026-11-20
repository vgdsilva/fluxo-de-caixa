using System.Security.Cryptography;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WebAPI.Common.Exceptions;
using WebAPI.Common.Messaging;
using WebAPI.Core.Messaging.Events;
using WebAPI.Data;
using WebAPI.Data.Entities;

namespace WebAPI.Core.Webhooks;

public sealed class WebhookService(
    WebAPIContext db,
    IOutbox outbox,
    IValidator<CreateWebhookSubscriptionRequest> validator) : IWebhookService
{
    public async Task<CreatedWebhookSubscriptionResponse> SubscribeAsync(
        CreateWebhookSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationFailedException(validation.ToDictionary().AsReadOnly());
        }

        var secret = string.IsNullOrWhiteSpace(request.Secret)
            ? Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32))
            : request.Secret;

        var subscription = new WebhookSubscription
        {
            Name = request.Name.Trim(),
            Url = request.Url.Trim(),
            Secret = secret,
            EventTypes = request.EventTypes.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
        };

        db.WebhookSubscriptions.Add(subscription);
        outbox.Add(new WebhookSubscriptionCreated(subscription.Id, subscription.Name, subscription.EventTypes));
        await db.SaveChangesAsync(cancellationToken);

        return new CreatedWebhookSubscriptionResponse(ToResponse(subscription), secret);
    }

    public async Task<IReadOnlyList<WebhookSubscriptionResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.WebhookSubscriptions.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return items.Select(ToResponse).ToList();
    }

    public async Task UnsubscribeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await db.WebhookSubscriptions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Assinatura {id} não encontrada.");

        subscription.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static WebhookSubscriptionResponse ToResponse(WebhookSubscription s) =>
        new(s.Id, s.Name, s.Url, s.EventTypes, s.IsActive, s.CreatedAt);
}
