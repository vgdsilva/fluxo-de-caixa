using WebAPI.Framework.Security;

namespace WebAPI.Test.Framework;

public sealed class HmacWebhookSignerTests
{
    private readonly HmacWebhookSigner _signer = new(TimeProvider.System);

    [Fact]
    public void Assinatura_valida_e_verificada()
    {
        var header = _signer.Sign("{\"a\":1}", "segredo-de-teste", DateTimeOffset.UtcNow);

        Assert.True(_signer.Verify("{\"a\":1}", "segredo-de-teste", header, TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void Payload_alterado_e_rejeitado()
    {
        var header = _signer.Sign("{\"a\":1}", "segredo-de-teste", DateTimeOffset.UtcNow);

        Assert.False(_signer.Verify("{\"a\":2}", "segredo-de-teste", header, TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void Assinatura_antiga_e_rejeitada()
    {
        var header = _signer.Sign("x", "segredo-de-teste", DateTimeOffset.UtcNow.AddHours(-1));

        Assert.False(_signer.Verify("x", "segredo-de-teste", header, TimeSpan.FromMinutes(5)));
    }
}
