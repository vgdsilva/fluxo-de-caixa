namespace WebAPI.Data.Entities;

/// <summary>Mensagem gravada na mesma transação do negócio e publicada depois pelo OutboxDispatcherService.</summary>
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public required string Type { get; set; }

    public required string Payload { get; set; }

    public string? CorrelationId { get; set; }

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ProcessedAt { get; set; }

    public int Attempts { get; set; }

    public string? LastError { get; set; }
}
