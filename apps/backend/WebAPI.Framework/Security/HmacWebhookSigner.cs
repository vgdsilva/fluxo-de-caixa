using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using WebAPI.Common.Webhooks;

namespace WebAPI.Framework.Security;

/// <summary>
/// Assinatura no formato "t={unix},v1={hex(HMACSHA256(secret, "{t}.{payload}"))}".
/// O timestamp no conteúdo assinado protege contra replay.
/// </summary>
public sealed class HmacWebhookSigner(TimeProvider timeProvider) : IWebhookSigner
{
    public string HeaderName => WebhookHeaders.Signature;

    public string Sign(string payload, string secret, DateTimeOffset timestamp)
    {
        var t = timestamp.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        return $"t={t},v1={Compute(payload, secret, t)}";
    }

    public bool Verify(string payload, string secret, string signatureHeader, TimeSpan tolerance)
    {
        string? t = null;
        string? v1 = null;
        foreach (var part in signatureHeader.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2) continue;
            if (kv[0] == "t") t = kv[1];
            else if (kv[0] == "v1") v1 = kv[1];
        }

        if (t is null || v1 is null || !long.TryParse(t, NumberStyles.None, CultureInfo.InvariantCulture, out var unix))
        {
            return false;
        }

        var age = timeProvider.GetUtcNow() - DateTimeOffset.FromUnixTimeSeconds(unix);
        if (age.Duration() > tolerance)
        {
            return false;
        }

        var expected = Encoding.ASCII.GetBytes(Compute(payload, secret, t));
        var received = Encoding.ASCII.GetBytes(v1.ToLowerInvariant());
        return CryptographicOperations.FixedTimeEquals(expected, received);
    }

    private static string Compute(string payload, string secret, string timestamp)
    {
        var data = Encoding.UTF8.GetBytes($"{timestamp}.{payload}");
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), data);
        return Convert.ToHexStringLower(hash);
    }
}
