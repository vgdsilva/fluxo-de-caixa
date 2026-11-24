using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        /// <summary>Emite o JWT da sessão e monta a resposta (usado também pelo refresh).</summary>
        public static Response Create(TokenManager tokenManager, User user, Session session, string refreshToken)
        {
            (string accessToken, DateTime accessTokenExpiresAt) = tokenManager.CreateJwtToken(user, session);
            return new Response(accessToken, accessTokenExpiresAt, refreshToken, session.ExpiresAt, UserDto.From(user));
        }
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

        /// <summary>
        /// Busca o usuário pelo login ou e-mail, confere a senha (bloqueia depois de erros seguidos),
        /// cria a sessão no banco e devolve o JWT + refresh token.
        /// </summary>
        private static async Task<Ok<Response>> Handle(
            Request request,
            WebAPIContext db,
            SessionManager sessionManager,
            TokenManager tokenManager,
            IPasswordHasher<User> passwordHasher,
            IOptions<AuthSessionOptions> options,
            TimeProvider timeProvider,
            HttpContext httpContext,
            CancellationToken cancellationToken)
        {
            string login = User.Normalize(request.Login);
            DateTime now = timeProvider.GetUtcNow().UtcDateTime;

            User? user = await db.Set<User>()
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Login == login || u.Email == login, cancellationToken);

            if (user is null)
            {
                // gasta o mesmo tempo de um hash para não revelar quais logins existem
                passwordHasher.HashPassword(new User { Name = "", Login = "", Email = "" }, request.Password);
                throw new InvalidCredentialsException();
            }

            if (!user.IsEnabled || user.LockedUntil > now)
                throw new InvalidCredentialsException();

            PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                user.FailedLoginCount++;
                if (user.FailedLoginCount >= options.Value.MaxFailedLogins)
                {
                    user.LockedUntil = now.AddMinutes(options.Value.LockoutMinutes);
                    user.FailedLoginCount = 0;
                }

                await db.SaveChangesAsync(cancellationToken);
                throw new InvalidCredentialsException();
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
                user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

            user.FailedLoginCount = 0;
            user.LockedUntil = null;
            user.LastLoginAt = now;
            await db.SaveChangesAsync(cancellationToken);

            (string refreshToken, Session session) = await sessionManager.CreateAsync(
                user,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.Request.Headers.UserAgent.ToString(),
                cancellationToken);

            return TypedResults.Ok(Response.Create(tokenManager, user, session, refreshToken));
        }
    }
}
