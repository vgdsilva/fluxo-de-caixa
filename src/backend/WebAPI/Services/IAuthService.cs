namespace WebAPI.Services;

/// <summary>Resultado de login/refresh: JWT de acesso + refresh token da sessão + usuário.</summary>
public sealed record AuthResult(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    User User);

/// <summary>Login com sessão no banco + JWT de vida curta.</summary>
public interface IAuthService
{
    /// <exception cref="InvalidCredentialsException">Login ou senha errados, usuário bloqueado ou desativado.</exception>
    Task<AuthResult> LoginAsync(string login, string password, string? ipAddress, string? userAgent,
                                     CancellationToken cancellationToken = default);

    /// <exception cref="SessionExpiredException">Refresh token inválido, revogado ou expirado.</exception>
    Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
