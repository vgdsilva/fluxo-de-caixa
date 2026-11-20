using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Core.Webhooks;

namespace WebAPI.Controllers.Webhooks;

/// <summary>Cadastro de assinaturas para webhooks enviados (outbound).</summary>
[ApiController]
[Route("api/webhooks/subscriptions")]
[Authorize]
public sealed class WebhookSubscriptionsController(IWebhookService webhookService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WebhookSubscriptionResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await webhookService.ListAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CreatedWebhookSubscriptionResponse>> Create(
        CreateWebhookSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var created = await webhookService.SubscribeAsync(request, cancellationToken);
        return Created($"api/webhooks/subscriptions/{created.Subscription.Id}", created);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await webhookService.UnsubscribeAsync(id, cancellationToken);
        return NoContent();
    }
}
