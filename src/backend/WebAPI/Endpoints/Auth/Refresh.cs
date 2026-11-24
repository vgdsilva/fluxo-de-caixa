using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WebAPI.Endpoints.Auth;

/// <summary>POST /api/auth/refresh: troca o refresh token por um novo (o antigo deixa de valer) e emite outro JWT.</summary>
public static class Refresh
{
    public sealed record Request(string RefreshToken);

    // A resposta tem o mesmo formato do login: Login.Response.

    public sealed class Validation : AbstractValidator<Request>
    {
        public Validation() => RuleFor(r => r.RefreshToken).NotEmpty().MaximumLength(200);
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder api) =>
            api.MapPost("/auth/refresh", Handle)
                 .WithTags("Auth")
                 .WithName("Refresh")
                 .WithSummary("Troca o refresh token por um novo e emite outro JWT.")
                 .WithValidation<Request>()
                 .RequireRateLimiting("AuthRateLimiter")
                 .AllowAnonymous();

        private static async Task<Ok<Login.Response>> Handle(
            Request request,
            SessionManager sessionManager,
            TokenManager tokenManager,
            CancellationToken cancellationToken)
        {
            (string Token, Session Session)? rotated = await sessionManager.RotateAsync(request.RefreshToken, cancellationToken);

            if (rotated is not { Session.User: { } user } value)
                throw new SessionExpiredException();

            return TypedResults.Ok(Login.Response.Create(tokenManager, user, value.Session, value.Token));
        }
    }
}
