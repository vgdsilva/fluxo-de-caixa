namespace WebAPI.Common.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "WebAPI";

    public string Audience { get; set; } = "WebAPI";

    /// <summary>Chave simétrica (mínimo 32 caracteres). Em produção, use user-secrets/variável de ambiente.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;
}
