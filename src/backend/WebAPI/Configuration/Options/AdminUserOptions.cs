namespace WebAPI.Configuration.Options;

/// <summary>
/// Primeiro usuário (administrador), criado no start se ainda não existir nenhum usuário.
/// Senha só por user-secrets ou variável de ambiente (<c>Admin__Password</c>).
/// </summary>
public sealed class AdminUserOptions
{
    public const string SectionName = "Admin";

    public string? Name { get; set; }

    /// <summary>Nome de acesso; padrão "admin".</summary>
    public string Login { get; set; } = "admin";

    public string? Email { get; set; }

    public string? Password { get; set; }
}
