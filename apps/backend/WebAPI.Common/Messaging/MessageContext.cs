namespace WebAPI.Common.Messaging;

/// <summary>Metadados da mensagem entregues ao handler.</summary>
public sealed record MessageContext(Guid MessageId, DateTimeOffset OccurredAt, string? CorrelationId);
