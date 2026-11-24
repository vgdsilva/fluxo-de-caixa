using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace WebAPI.Services.Impl;

/// <summary>
/// Login por nome de acesso (ou e-mail) e senha com bloqueio após erros seguidos. Cada login cria uma sessão no banco;
/// o JWT de acesso dura minutos e é renovado em <see cref="RefreshAsync"/> enquanto a sessão for válida.
/// </summary>
public class AuthService(WebAPIContext db,
                         SessionManager sessionManager,
                         TokenManager tokenManager,
                         IPasswordHasher<User> passwordHasher,
                         IOptions<AuthSessionOptions> options,
                         TimeProvider timeProvider) : IAuthService
{
    public static string Normalize(string value) => value.Trim().ToLowerInvariant();

    public async Task<AuthResult> LoginAsync(string login,
                                              string password,
                                              string? ipAddress,
                                              string? userAgent,
                                              CancellationToken cancellationToken = default)
    {
        string normalized = Normalize(login);
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;

        User? user = await db.Set<User>()
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Login == normalized || u.Email == normalized, cancellationToken);

        if (user is null)
        {
            // gasta o mesmo tempo de um hash para não revelar quais logins existem
            passwordHasher.HashPassword(new User { Name = "", Login = "", Email = "" }, password);
            throw new InvalidCredentialsException();
        }

        if (!user.IsEnabled || user.LockedUntil > now)
            throw new InvalidCredentialsException();

        PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

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
            user.PasswordHash = passwordHasher.HashPassword(user, password);

        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.LastLoginAt = now;
        await db.SaveChangesAsync(cancellationToken);

        (string refreshToken, Session session) = await sessionManager.CreateAsync(user, ipAddress, userAgent, cancellationToken);

        return Issue(user, session, refreshToken);
    }

    /// <summary>Troca o token da sessão por um novo e emite outro JWT. Lança <see cref="SessionExpiredException"/> se a sessão não vale mais.</summary>
    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        (string Token, Session Session)? rotated = await sessionManager.RotateAsync(refreshToken, cancellationToken);

        if (rotated is not { Session.User: { } user } value)
            throw new SessionExpiredException();

        return Issue(user, value.Session, value.Token);
    }

    public Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        sessionManager.RevokeAsync(refreshToken, cancellationToken);

    private AuthResult Issue(User user, Session session, string refreshToken)
    {
        (string accessToken, DateTime accessExpiresAt) = tokenManager.CreateJwtToken(user, session);
        return new AuthResult(accessToken, accessExpiresAt, refreshToken, session.ExpiresAt, user);
    }
}
