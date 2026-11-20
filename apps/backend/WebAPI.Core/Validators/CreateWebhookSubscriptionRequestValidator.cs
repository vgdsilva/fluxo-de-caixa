using FluentValidation;
using WebAPI.Core.Webhooks;

namespace WebAPI.Core.Validators;

public sealed class CreateWebhookSubscriptionRequestValidator : AbstractValidator<CreateWebhookSubscriptionRequest>
{
    public CreateWebhookSubscriptionRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(2000)
            .Must(BeHttpUrl).WithMessage("Informe uma URL http(s) válida.");
        RuleFor(x => x.EventTypes).NotEmpty().WithMessage("Informe ao menos um tipo de evento (ou \"*\").");
        RuleForEach(x => x.EventTypes).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Secret).MinimumLength(16).When(x => !string.IsNullOrEmpty(x.Secret));
    }

    private static bool BeHttpUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
