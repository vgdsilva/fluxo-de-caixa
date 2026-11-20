namespace WebAPI.Common.Configuration;

public sealed class WebhookOptions
{
    public const string SectionName = "Webhooks";

    public int MaxAttempts { get; set; } = 8;

    public int BaseDelaySeconds { get; set; } = 30;

    public int MaxDelayMinutes { get; set; } = 60;

    public int TimeoutSeconds { get; set; } = 15;

    /// <summary>Tolerância de relógio para validar assinaturas recebidas.</summary>
    public int SignatureToleranceSeconds { get; set; } = 300;

    /// <summary>Segredos de webhooks recebidos (inbound), por origem. Ex.: "banco" → "segredo".</summary>
    public Dictionary<string, string> InboundSecrets { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
