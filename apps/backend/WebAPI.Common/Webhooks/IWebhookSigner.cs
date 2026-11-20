namespace WebAPI.Common.Webhooks;

/// <summary>Assinatura de payload de webhook (HMAC). Implementação em Framework/Security.</summary>
public interface IWebhookSigner
{
    /// <summary>Nome do header HTTP onde a assinatura vai/vem.</summary>
    string HeaderName { get; }

    string Sign(string payload, string secret, DateTimeOffset timestamp);

    bool Verify(string payload, string secret, string signatureHeader, TimeSpan tolerance);
}
