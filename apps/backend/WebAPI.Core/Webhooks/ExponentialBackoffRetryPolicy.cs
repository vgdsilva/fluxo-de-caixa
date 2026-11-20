using Microsoft.Extensions.Options;
using WebAPI.Common.Configuration;
using WebAPI.Common.Webhooks;

namespace WebAPI.Core.Webhooks;

/// <summary>Backoff exponencial com jitter: base * 2^(n-1), limitado a MaxDelayMinutes.</summary>
public sealed class ExponentialBackoffRetryPolicy(IOptions<WebhookOptions> options) : IWebhookRetryPolicy
{
    public int MaxAttempts => options.Value.MaxAttempts;

    public DateTimeOffset? GetNextAttempt(int attemptsSoFar, DateTimeOffset now)
    {
        if (attemptsSoFar >= MaxAttempts)
        {
            return null;
        }

        var o = options.Value;
        var exponent = Math.Min(Math.Max(attemptsSoFar - 1, 0), 20);
        var delaySeconds = Math.Min(o.BaseDelaySeconds * Math.Pow(2, exponent), o.MaxDelayMinutes * 60d);
        var jitter = Random.Shared.NextDouble() * 0.2 * delaySeconds;
        return now.AddSeconds(delaySeconds + jitter);
    }
}
