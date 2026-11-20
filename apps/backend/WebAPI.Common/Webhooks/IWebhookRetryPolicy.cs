namespace WebAPI.Common.Webhooks;

/// <summary>Define quando tentar de novo uma entrega que falhou.</summary>
public interface IWebhookRetryPolicy
{
    int MaxAttempts { get; }

    /// <summary>Retorna o próximo horário de tentativa, ou null se esgotou as tentativas.</summary>
    DateTimeOffset? GetNextAttempt(int attemptsSoFar, DateTimeOffset now);
}
