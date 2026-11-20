namespace WebAPI.Common.Configuration;

public sealed class BackgroundOptions
{
    public const string SectionName = "Background";

    /// <summary>
    /// Liga os BackgroundServices neste host. Deixe false na WebAPI quando o WebAPI.Worker estiver rodando.
    /// </summary>
    public bool Enabled { get; set; } = true;

    public int OutboxPollingSeconds { get; set; } = 5;

    public int OutboxBatchSize { get; set; } = 50;

    public int WebhookPollingSeconds { get; set; } = 10;

    public int WebhookBatchSize { get; set; } = 20;
}
