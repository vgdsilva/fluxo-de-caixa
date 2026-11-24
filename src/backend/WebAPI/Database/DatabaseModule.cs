namespace WebAPI.Database;

public static class DatabaseModule
{
    /// <summary>
    /// Adiciona o contexto do banco. A configuração do EF (conexão, interceptors, tracking)
    /// fica no próprio <see cref="WebAPIContext"/>; aqui só entram as opções da seção "Database".
    /// </summary>
    public static IServiceCollection AddDatabaseModule(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Host) && !string.IsNullOrWhiteSpace(o.Name),
                      "Configure Database:Host e Database:Name.")
            .ValidateOnStart();

        // Sem pooling: o WebAPIContext recebe DatabaseOptions e IHttpContextAccessor pelo construtor
        // e se configura no OnConfiguring. Fora de uma requisição (startup, background), crie um escopo
        // com IServiceScopeFactory e peça o WebAPIContext a ele.
        services.AddDbContext<WebAPIContext>();

        services.AddStartupTask<DatabaseMigratorStartup>();

        return services;
    }
}
