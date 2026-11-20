namespace WebAPI.Data.Entities;

/// <summary>Cadastro de um destino que quer receber eventos (outbound).</summary>
public class WebhookSubscription : EntityBase
{
    public required string Name { get; set; }

    public required string Url { get; set; }

    /// <summary>Segredo usado para assinar (HMAC) o payload enviado.</summary>
    public required string Secret { get; set; }

    /// <summary>Tipos de evento assinados. "*" assina todos.</summary>
    public List<string> EventTypes { get; set; } = [];

    public bool IsActive { get; set; } = true;

    public ICollection<WebhookDelivery> Deliveries { get; set; } = [];

    public bool Accepts(string eventType) =>
        EventTypes.Contains("*") || EventTypes.Contains(eventType, StringComparer.OrdinalIgnoreCase);
}
