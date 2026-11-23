using Npgsql;

namespace WebAPI.Configuration.Options;

/// <summary>
/// Conexão com o PostgreSQL, campo a campo (seção "Database" do appsettings).
/// A senha não fica no appsettings: em desenvolvimento use
/// <c>dotnet user-secrets set "Database:Password" "..."</c>; em produção a variável <c>Database__Password</c>.
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 5432;

    public string Name { get; set; } = "fluxo_de_caixa";

    public string Username { get; set; } = "postgres";

    public string? Password { get; set; }

    /// <summary>Tempo máximo, em segundos, para abrir a conexão.</summary>
    public int Timeout { get; set; } = 15;

    /// <summary>Mostra parâmetros de SQL e erros detalhados nos logs. Só para desenvolvimento.</summary>
    public bool EnableSensitiveDataLogging { get; set; }

    public string BuildConnectionString() => new NpgsqlConnectionStringBuilder
    {
        Host = Host,
        Port = Port,
        Database = Name,
        Username = Username,
        Password = Password,
        Timeout = Timeout,
    }.ConnectionString;
}
