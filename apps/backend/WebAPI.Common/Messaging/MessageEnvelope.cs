namespace WebAPI.Common.Messaging;

/// <summary>
/// Envelope serializado que trafega pela fila. <see cref="Type"/> é o nome lógico da mensagem
/// (ver <see cref="MessageTypeAttribute"/>) e <see cref="Payload"/> é o JSON do corpo.
/// </summary>
public sealed record MessageEnvelope(
    Guid Id,
    string Type,
    string Payload,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null);
