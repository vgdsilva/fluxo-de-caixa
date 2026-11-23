using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebAPI.Configuration.Options;
using WebAPI.Data;
using WebAPI.Data.Types;

namespace WebAPI.Core.Utils.Security;

/// <summary>
/// Sessões de login gravadas no banco. O cliente recebe um token aleatório (refresh token);
/// o banco guarda só o hash SHA-256. Cada renovação troca o token (rotação), e revogar é marcar
/// <see cref="Session.RevokedAt"/>.
/// </summary>
public class SessionManager(WebAPIContext db, IOptions<AuthSessionOptions> options, TimeProvider timeProvider)
{
    private AuthSessionOptions Options => options.Value;

    /// <summary>Cria a sessão e devolve o token em claro (só existe neste momento; o banco guarda o hash).</summary>
    public async Task<(string Token, Session Session)> CreateAsync(User user,
                                                                   string? ipAddress,
                                                                   string? userAgent,
                                                                   CancellationToken cancellationToken = default)
    {
        string token = NewToken();
        DateTime now = Now();

        Session session = new()
        {
            TokenHash = Hash(token),
            UserId = user.Id,
            User = user,
            LastAccessAt = now,
            ExpiresAt = now.AddDays(Options.AbsoluteTimeoutDays),
            IpAddress = ipAddress,
            UserAgent = userAgent is { Length: > 300 } ? userAgent[..300] : userAgent,
        };

        db.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        return (token, session);
    }

    /// <summary>
    /// Valida o token da sessão e troca por um novo. Devolve null se a sessão não existir,
    /// estiver revogada, expirada, ociosa demais ou se o usuário estiver desativado.
    /// </summary>
    public async Task<(string Token, Session Session)?> RotateAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        string hash = Hash(token);
        DateTime now = Now();

        Session? session = await db.Set<Session>()
            .AsTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.TokenHash == hash, cancellationToken);

        if (session is null ||
            session.RevokedAt is not null ||
            session.ExpiresAt <= now ||
            session.LastAccessAt.AddMinutes(Options.IdleTimeoutMinutes) <= now ||
            session.User is not { IsEnabled: true })
        {
            return null;
        }

        string newToken = NewToken();
        session.TokenHash = Hash(newToken);
        session.LastAccessAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return (newToken, session);
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        string hash = Hash(token);

        Session? session = await db.Set<Session>()
            .AsTracking()
            .FirstOrDefaultAsync(s => s.TokenHash == hash && s.RevokedAt == null, cancellationToken);

        if (session is null)
            return;

        session.RevokedAt = Now();
        await db.SaveChangesAsync(cancellationToken);
    }

    public static string Hash(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private DateTime Now() => timeProvider.GetUtcNow().UtcDateTime;

    private static string NewToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
