using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebAPI.Common.Configuration;
using WebAPI.Common.Messaging;
using WebAPI.Data;

namespace WebAPI.Framework.Background;

/// <summary>Lê o outbox (mensagens ainda não processadas) e publica na fila.</summary>
public sealed class OutboxDispatcherService(
    IServiceScopeFactory scopeFactory,
    IMessagePublisher publisher,
    IOptions<BackgroundOptions> options,
    ILogger<OutboxDispatcherService> logger) : BackgroundService
{
    private const int MaxAttempts = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(options.Value.OutboxPollingSeconds);
        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falha ao processar o outbox.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DispatchBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<WebAPIContext>();

        var batch = await db.OutboxMessages
            .Where(x => x.ProcessedAt == null && x.Attempts < MaxAttempts)
            .OrderBy(x => x.OccurredAt)
            .Take(options.Value.OutboxBatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in batch)
        {
            try
            {
                await publisher.PublishAsync(
                    new MessageEnvelope(message.Id, message.Type, message.Payload, message.OccurredAt, message.CorrelationId),
                    cancellationToken);
                message.ProcessedAt = DateTimeOffset.UtcNow;
                message.LastError = null;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                message.Attempts++;
                message.LastError = ex.Message;
                logger.LogWarning(ex, "Falha ao publicar a mensagem {MessageId} ({Type}).", message.Id, message.Type);
            }
        }

        if (batch.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
