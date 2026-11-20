using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebAPI.Common.Messaging;
using WebAPI.Data;
using WebAPI.Data.Entities;
using WebAPI.Framework.Messaging;

namespace WebAPI.Framework.Background;

/// <summary>Consome a fila e entrega cada mensagem aos handlers, com idempotência via InboxMessage.</summary>
public sealed class QueueConsumerService(
    IServiceScopeFactory scopeFactory,
    IMessageQueueReader reader,
    MessageDispatcher dispatcher,
    ILogger<QueueConsumerService> logger) : BackgroundService
{
    private const string ConsumerName = nameof(QueueConsumerService);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var envelope in reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await HandleAsync(envelope, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falha ao processar a mensagem {MessageId} ({Type}).", envelope.Id, envelope.Type);
            }
        }
    }

    private async Task HandleAsync(MessageEnvelope envelope, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<WebAPIContext>();

        var inbox = await db.InboxMessages
            .FirstOrDefaultAsync(x => x.MessageId == envelope.Id && x.Consumer == ConsumerName, cancellationToken);

        if (inbox?.ProcessedAt is not null)
        {
            logger.LogDebug("Mensagem {MessageId} já processada; ignorando.", envelope.Id);
            return;
        }

        if (inbox is null)
        {
            inbox = new InboxMessage { MessageId = envelope.Id, Consumer = ConsumerName, Type = envelope.Type };
            db.InboxMessages.Add(inbox);
        }

        try
        {
            var handled = await dispatcher.DispatchAsync(scope.ServiceProvider, envelope, cancellationToken);
            if (!handled)
            {
                logger.LogWarning("Nenhum tipo registrado para a mensagem {Type}.", envelope.Type);
            }

            inbox.ProcessedAt = DateTimeOffset.UtcNow;
            inbox.LastError = null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            inbox.LastError = ex.Message;
            throw;
        }
        finally
        {
            await db.SaveChangesAsync(CancellationToken.None);
        }
    }
}
