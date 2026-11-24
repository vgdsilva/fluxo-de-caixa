namespace WebAPI.Config.Options;

/// <summary>
/// Sessão de login gravada no banco (seção "Session" do appsettings). O token da sessão (refresh token)
/// volta no corpo do login e serve só para emitir novos JWTs em /api/auth/refresh.
/// </summary>
public sealed class AuthSessionOptions
{
    public const string SectionName = "Session";

    /// <summary>Sem nenhuma renovação por este tempo, a sessão expira.</summary>
    public int IdleTimeoutMinutes { get; set; } = 60 * 24 * 7;

    /// <summary>Validade máxima da sessão, mesmo em uso contínuo.</summary>
    public int AbsoluteTimeoutDays { get; set; } = 30;

    /// <summary>Tentativas de senha erradas seguidas antes de bloquear o usuário.</summary>
    public int MaxFailedLogins { get; set; } = 5;

    /// <summary>Tempo de bloqueio depois de <see cref="MaxFailedLogins"/> erros.</summary>
    public int LockoutMinutes { get; set; } = 15;
}
