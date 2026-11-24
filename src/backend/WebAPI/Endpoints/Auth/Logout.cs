using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WebAPI.Endpoints.Auth;

/// <summary>POST /api/auth/logout: encerra a sessão do refresh token. Responde 204 mesmo se ela já não existir.</summary>
public static class Logout
{
    public sealed record Request(string RefreshToken);

    public sealed class Validation : AbstractValidator<Request>
    {
        public Validation() => RuleFor(r => r.RefreshToken).NotEmpty().MaximumLength(200);
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder api) =>
            api.MapPost("/auth/logout", Handle)
                 .WithTags("Auth")
                 .WithName("Logout")
                 .WithSummary("Encerra a sessão do refresh token.")
                 .WithValidation<Request>()
                 .AllowAnonymous();

        private static async Task<NoContent> Handle(
            Request request,
            IAuthService authService,
            CancellationToken cancellationToken)
        {
            await authService.LogoutAsync(request.RefreshToken, cancellationToken);
            return TypedResults.NoContent();
        }
    }
}
