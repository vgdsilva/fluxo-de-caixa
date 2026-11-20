namespace WebAPI.Data.Entities;

/// <summary>
/// Registro de mensagens já processadas (idempotência). Antes de rodar os handlers, o consumidor
/// verifica se o par (MessageId, Consumer) já existe.
/// </summary>
public class InboxMessage
{
    public Guid MessageId { get; set; }

    public required string Consumer { get; set; }

    public required string Type { get; set; }

    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ProcessedAt { get; set; }

    public string? LastError { get; set; }
}
