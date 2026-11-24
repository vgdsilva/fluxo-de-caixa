using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WebAPI.Endpoints.Auth;

/// <summary>POST /api/auth/login: login por nome de acesso (o e-mail também serve) e senha.</summary>
public static class Login
{
    public sealed record Request(string Login, string Password);

    /// <summary>
    /// O <c>AccessToken</c> vai no header <c>Authorization: Bearer</c> até <c>AccessTokenExpiresAt</c>;
    /// antes disso, chame /api/auth/refresh com o <c>RefreshToken</c>.
    /// </summary>
    public sealed record Response(
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt,
        UserDto User)
    {
        public static Response From(AuthResult result) => new(
            result.AccessToken,
            result.AccessTokenExpiresAt,
            result.RefreshToken,
            result.RefreshTokenExpiresAt,
            UserDto.From(result.User));
    }

    public sealed class Validation : AbstractValidator<Request>
    {
        public Validation()
        {
            RuleFor(r => r.Login).NotEmpty().MaximumLength(254);
            RuleFor(r => r.Password).NotEmpty().MaximumLength(200);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder api) =>
            api.MapPost("/auth/login", Handle)
                 .WithTags("Auth")
                 .WithName("Login")
                 .WithSummary("Login por nome de acesso (ou e-mail) e senha.")
                 .WithValidation<Request>()
                 .RequireRateLimiting("AuthRateLimiter")
                 .AllowAnonymous();

        private static async Task<Ok<Response>> Handle(
            Request request,
            IAuthService authService,
            HttpContext httpContext,
            CancellationToken cancellationToken)
        {
            AuthResult result = await authService.LoginAsync(
                request.Login,
                request.Password,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.Request.Headers.UserAgent.ToString(),
                cancellationToken);

            return TypedResults.Ok(Response.From(result));
        }
    }
}
