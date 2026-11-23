using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebAPI.Configuration.Options;
using WebAPI.Core.Utils.Security;
using WebAPI.Data;
using WebAPI.Data.Types;

namespace WebAPI.Core.Services;

/// <summary>Resultado de login/refresh: JWT de acesso + token da sessão (vai para o cookie).</summary>
public sealed record AuthResult(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    User User);

/// <summary>
/// Login por e-mail e senha com bloqueio após erros seguidos. Cada login cria uma sessão no banco;
/// o JWT de acesso dura minutos e é renovado em <see cref="RefreshAsync"/> enquanto a sessão for válida.
/// </summary>
public class AuthService(WebAPIContext db,
                         SessionManager sessionManager,
                         TokenManager tokenManager,
                         IPasswordHasher<User> passwordHasher,
                         IOptions<AuthSessionOptions> options,
                         TimeProvider timeProvider)
{
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public async Task<AuthResult?> LoginAsync(string email,
                                              string password,
                                              string? ipAddress,
                                              string? userAgent,
                                              CancellationToken cancellationToken = default)
    {
        string normalized = NormalizeEmail(email);
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;

        User? user = await db.Set<User>()
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);

        if (user is null)
        {
            // gasta o mesmo tempo de um hash para não revelar quais e-mails existem
            passwordHasher.HashPassword(new User { Name = "", Email = "" }, password);
            return null;
        }

        if (!user.IsEnabled || user.LockedUntil > now)
            return null;

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
            return null;
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

    /// <summary>Troca o token da sessão por um novo e emite outro JWT. Null se a sessão não vale mais.</summary>
    public async Task<AuthResult?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        (string Token, Session Session)? rotated = await sessionManager.RotateAsync(refreshToken, cancellationToken);

        if (rotated is not { Session.User: { } user } value)
            return null;

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
