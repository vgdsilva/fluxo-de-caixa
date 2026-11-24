using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WebAPI.Modules.Auth.Endpoints;

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
        public void Map(IEndpointRouteBuilder group) =>
            group.MapPost("/refresh", Handle)
                 .WithName("Refresh")
                 .WithSummary("Troca o refresh token por um novo e emite outro JWT.")
                 .WithValidation<Request>()
                 .RequireRateLimiting("AuthRateLimiter");

        private static async Task<Results<Ok<Login.Response>, ProblemHttpResult>> Handle(
            Request request,
            AuthService authService,
            CancellationToken cancellationToken)
        {
            AuthResult? result = await authService.RefreshAsync(request.RefreshToken, cancellationToken);

            if (result is null)
                return TypedResults.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Sessão expirada. Faça login novamente.");

            return TypedResults.Ok(Login.Response.From(result));
        }
    }
}
