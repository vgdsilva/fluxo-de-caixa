using Microsoft.Extensions.Options;
using WebAPI.Common.Configuration;
using WebAPI.Core.Webhooks;

namespace WebAPI.Test.Core;

public sealed class ExponentialBackoffRetryPolicyTests
{
    private static ExponentialBackoffRetryPolicy Create() =>
        new(Options.Create(new WebhookOptions { MaxAttempts = 3, BaseDelaySeconds = 10, MaxDelayMinutes = 1 }));

    [Fact]
    public void Atraso_cresce_e_respeita_limite()
    {
        var now = DateTimeOffset.UtcNow;
        var policy = Create();

        var first = policy.GetNextAttempt(1, now)!.Value - now;
        var second = policy.GetNextAttempt(2, now)!.Value - now;

        Assert.InRange(first.TotalSeconds, 10, 12);
        Assert.InRange(second.TotalSeconds, 20, 24);
    }

    [Fact]
    public void Esgota_apos_max_tentativas()
    {
        Assert.Null(Create().GetNextAttempt(3, DateTimeOffset.UtcNow));
    }
}
