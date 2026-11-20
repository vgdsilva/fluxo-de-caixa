using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebAPI.Common.Configuration;
using WebAPI.Common.Webhooks;
using WebAPI.Data;
using WebAPI.Data.Entities;

namespace WebAPI.Framework.Background;

/// <summary>Entrega webhooks pendentes via HTTP, com assinatura HMAC e retry/backoff.</summary>
public sealed class WebhookDeliveryService(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IWebhookSigner signer,
    IWebhookRetryPolicy retryPolicy,
    IOptions<BackgroundOptions> options,
    ILogger<WebhookDeliveryService> logger) : BackgroundService
{
    public const string HttpClientName = "webhooks";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.WebhookPollingSeconds));

        do
        {
            try
            {
                await DeliverBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falha ao processar entregas de webhook.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DeliverBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<WebAPIContext>();
        var now = DateTimeOffset.UtcNow;

        var deliveries = await db.WebhookDeliveries
            .Include(x => x.Subscription)
            .Where(x => x.Status == WebhookDeliveryStatus.Pending && x.NextAttemptAt <= now)
            .OrderBy(x => x.NextAttemptAt)
            .Take(options.Value.WebhookBatchSize)
            .ToListAsync(cancellationToken);

        foreach (var delivery in deliveries)
        {
            await DeliverAsync(delivery, cancellationToken);
        }

        if (deliveries.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task DeliverAsync(WebhookDelivery delivery, CancellationToken cancellationToken)
    {
        var subscription = delivery.Subscription;
        if (subscription is null || !subscription.IsActive)
        {
            delivery.Status = WebhookDeliveryStatus.Failed;
            delivery.LastError = "Assinatura inativa ou removida.";
            return;
        }

        delivery.Attempts++;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, subscription.Url)
            {
                Content = new StringContent(delivery.Payload, Encoding.UTF8, "application/json"),
            };
            request.Headers.Add(signer.HeaderName, signer.Sign(delivery.Payload, subscription.Secret, DateTimeOffset.UtcNow));
            request.Headers.Add(WebhookHeaders.EventType, delivery.EventType);
            request.Headers.Add(WebhookHeaders.DeliveryId, delivery.Id.ToString());

            var client = httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.SendAsync(request, cancellationToken);
            delivery.LastStatusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                delivery.Status = WebhookDeliveryStatus.Succeeded;
                delivery.DeliveredAt = DateTimeOffset.UtcNow;
                delivery.LastError = null;
                return;
            }

            ScheduleRetry(delivery, $"HTTP {(int)response.StatusCode}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            ScheduleRetry(delivery, ex.Message);
        }
    }

    private void ScheduleRetry(WebhookDelivery delivery, string error)
    {
        delivery.LastError = error;
        var next = retryPolicy.GetNextAttempt(delivery.Attempts, DateTimeOffset.UtcNow);
        if (next is null)
        {
            delivery.Status = WebhookDeliveryStatus.Failed;
            logger.LogWarning("Webhook {DeliveryId} falhou definitivamente após {Attempts} tentativas.", delivery.Id, delivery.Attempts);
        }
        else
        {
            delivery.NextAttemptAt = next.Value;
        }
    }
}
