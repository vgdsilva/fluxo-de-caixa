using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WebAPI.Configuration.Options;
using WebAPI.Data.Types;

namespace WebAPI.Core.Utils.Security;

/// <summary>
/// Emite e valida o JWT de acesso (HMAC-SHA256). A chave vem de <see cref="JwtOptions.SigningKey"/>,
/// então reiniciar a API não invalida os tokens emitidos.
/// </summary>
public class TokenManager
{
    public const string SessionIdClaim = "session_id";

    private readonly JwtOptions _options;
    private readonly SymmetricSecurityKey _key;
    private readonly TimeProvider _timeProvider;
    private readonly JsonWebTokenHandler _handler = new() { MapInboundClaims = false };

    public TokenManager(IOptions<JwtOptions> options, IHostEnvironment environment, TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;

        string? signingKey = _options.SigningKey;

        if (string.IsNullOrWhiteSpace(signingKey))
        {
            if (!environment.IsDevelopment())
                throw new InvalidOperationException("Configure Jwt:SigningKey (mínimo 32 caracteres).");

            // Em desenvolvimento sem chave configurada: chave aleatória por execução (tokens caem no restart).
            Log.Warning("Jwt:SigningKey não configurada; usando chave temporária de desenvolvimento.");
            signingKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        }

        byte[] keyBytes = Encoding.UTF8.GetBytes(signingKey);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey precisa ter pelo menos 32 caracteres.");

        _key = new SymmetricSecurityKey(keyBytes);
    }

    public int AccessTokenMinutes => _options.AccessTokenMinutes;

    public virtual (string Token, DateTime ExpiresAt) CreateJwtToken(User user, Session session)
    {
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        DateTime expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

        SecurityTokenDescriptor descriptor = new()
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Sid, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Login),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(SessionIdClaim, session.Id.ToString()),
            ]),
        };

        return (_handler.CreateToken(descriptor), expiresAt);
    }

    /// <summary>Valida assinatura, emissor, audiência e expiração. Devolve null se o token for inválido.</summary>
    public virtual async Task<ClaimsIdentity?> ValidateJwtTokenAsync(string jwtToken, string authenticationType)
    {
        TokenValidationResult result = await _handler.ValidateTokenAsync(jwtToken, new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            LifetimeValidator = (notBefore, expires, _, _) =>
            {
                DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
                return (notBefore is null || notBefore <= now.AddSeconds(30)) && expires > now.AddSeconds(-30);
            },
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            AuthenticationType = authenticationType,
        });

        return result.IsValid ? result.ClaimsIdentity : null;
    }
}
