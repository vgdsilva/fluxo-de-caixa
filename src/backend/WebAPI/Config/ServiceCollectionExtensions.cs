using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;

namespace WebAPI.Config;

public static class ServiceCollectionExtensions
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
    {
        var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        HttpContextHolder.Configure(httpContextAccessor);
        return app;
    }

    /// <summary>
    /// Adiciona serviços utilizados pelo sistema
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddProjectServices(this IServiceCollection services)
    {
        services
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<IDataSeeder, AdminUserSeeder>();

        return services;
    }

    /// <summary>
    /// Adiciona o contexto do banco. A configuração do EF (conexão, interceptors, tracking)
    /// fica no próprio <see cref="WebAPIContext"/>; aqui só entram as opções da seção "Database".
    /// </summary>
    /// <param name="services">Interface de serviços</param>
    /// <param name="configuration">Configurações da aplicação</param>
    /// <returns>Interface de serviços configurada</returns>
    public static IServiceCollection AddProjectDatabaseContext(this IServiceCollection services,
                                                               IConfiguration configuration)
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

        return services;
    }

    /// <summary>
    /// Adiciona a autenticação: sessão no banco (<see cref="SessionManager"/>) + JWT de acesso
    /// de vida curta (<see cref="TokenManager"/>), validado em cada requisição pelo <see cref="CustomAuthHandler"/>.
    /// Todo endpoint em /api exige usuário autenticado, exceto os marcados com .AllowAnonymous().
    /// </summary>
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services,
                                                             IConfiguration configuration)
    {
        services.Configure<AuthSessionOptions>(configuration.GetSection(AuthSessionOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AdminUserOptions>(configuration.GetSection(AdminUserOptions.SectionName));

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

        services
            .AddSingleton<TokenManager>()
            .AddScoped<SessionManager>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CustomAuthOptions.DefaultScheme;
                options.DefaultChallengeScheme = CustomAuthOptions.DefaultScheme;
            })
            .AddScheme<CustomAuthOptions, CustomAuthHandler>(CustomAuthOptions.DefaultScheme, null);

        // Toda rota em /api exige usuário autenticado (ver EndpointExtensions.MapApi); pública usa .AllowAnonymous().
        services.AddAuthorization();

        services
            .AddScoped<UserInfo>();

        return services;
    }

    /// <summary>
    /// Adiciona tarefas a serem inicializadas durante o startup da aplicação
    /// </summary>
    public static IServiceCollection AddStartupTask<T>(this IServiceCollection services) where T : class, IStartupTask => services.AddTransient<IStartupTask, T>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public static void AddFileProvider(this IServiceCollection services)
    {
        IFileProvider physicalProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
        services.AddSingleton<IFileProvider>(physicalProvider);
    }
}
