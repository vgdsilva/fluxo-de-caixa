using System.Text.Json;
using WebAPI.Common.Converters;
using WebAPI.Common.Messaging;
using WebAPI.Data.Entities;

namespace WebAPI.Data.Repository;

/// <summary>Adiciona a mensagem ao DbContext; ela é gravada junto com o próximo SaveChanges.</summary>
public sealed class Outbox(WebAPIContext context) : IOutbox
{
    public void Add<TMessage>(TMessage message, string? correlationId = null) where TMessage : IMessage
    {
        context.OutboxMessages.Add(new OutboxMessage
        {
            Type = MessageTypeAttribute.GetName(typeof(TMessage)),
            Payload = JsonSerializer.Serialize(message, JsonDefaults.Options),
            CorrelationId = correlationId,
        });
    }
}
