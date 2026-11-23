namespace WebAPI.Configuration.Options;

/// <summary>
/// Token de acesso (JWT) de vida curta, renovado pela sessão (seção "Jwt" do appsettings).
/// A chave de assinatura não fica no appsettings: use <c>dotnet user-secrets set "Jwt:SigningKey" "..."</c>
/// ou a variável <c>Jwt__SigningKey</c> (mínimo 32 caracteres).
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "fluxo-de-caixa";

    public string Audience { get; set; } = "fluxo-de-caixa";

    public string? SigningKey { get; set; }

    /// <summary>Validade do token de acesso. Depois disso o front chama /api/auth/refresh.</summary>
    public int AccessTokenMinutes { get; set; } = 15;
}
