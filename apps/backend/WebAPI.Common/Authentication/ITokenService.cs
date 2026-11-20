namespace WebAPI.Common.Authentication;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>Emissão de tokens JWT. Implementação em Framework/Security.</summary>
public interface ITokenService
{
    AccessToken CreateToken(string userId, string name, IEnumerable<string>? roles = null);
}
