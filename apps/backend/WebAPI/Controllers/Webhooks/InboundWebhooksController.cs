using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebAPI.Common.Configuration;
using WebAPI.Common.Messaging;
using WebAPI.Common.Webhooks;
using WebAPI.Data;
using WebAPI.Data.Entities;

namespace WebAPI.Controllers.Webhooks;

/// <summary>
/// Recebimento de webhooks de terceiros (ex.: banco). Valida HMAC, grava no outbox
/// e responde 202 rápido; o processamento acontece pelos handlers da fila.
/// </summary>
[ApiController]
[Route("api/webhooks/inbound")]
[AllowAnonymous]
public sealed class InboundWebhooksController(
    WebAPIContext db,
    IWebhookSigner signer,
    IOptions<WebhookOptions> options,
    ILogger<InboundWebhooksController> logger) : ControllerBase
{
    private const int MaxBodyBytes = 512 * 1024;

    [HttpPost("{source}")]
    [RequestSizeLimit(MaxBodyBytes)]
    public async Task<IActionResult> Receive(string source, CancellationToken cancellationToken)
    {
        if (!options.Value.InboundSecrets.TryGetValue(source, out var secret) || string.IsNullOrEmpty(secret))
        {
            return NotFound();
        }

        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers[signer.HeaderName].FirstOrDefault();
        var tolerance = TimeSpan.FromSeconds(options.Value.SignatureToleranceSeconds);

        if (signature is null || !signer.Verify(payload, secret, signature, tolerance))
        {
            logger.LogWarning("Webhook de {Source} com assinatura inválida.", source);
            return Unauthorized();
        }

        var eventType = Request.Headers[WebhookHeaders.EventType].FirstOrDefault() ?? "unknown";
        db.OutboxMessages.Add(new OutboxMessage
        {
            Type = $"inbound.{source}.{eventType}",
            Payload = payload,
            CorrelationId = HttpContext.TraceIdentifier,
        });
        await db.SaveChangesAsync(cancellationToken);

        return Accepted();
    }
}
