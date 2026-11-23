using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using WebAPI.Configuration.Options;
using WebAPI.Core.Seeders;
using WebAPI.Core.Services;
using WebAPI.Core.Utils.Extensions;
using WebAPI.Core.Utils.Interfaces;
using WebAPI.Core.Utils.Security;
using WebAPI.Data;
using WebAPI.Data.Types;

namespace WebAPI;

public static class ProjectServiceCollectionExtensions
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
    /// Add models.
    /// </summary>
    /// <param name="services">Repositórios</param>
    /// <returns>Collection de repositórios</returns>
    public static IServiceCollection AddModels(this IServiceCollection services)
    {
        //services
        //    .AddTransient<DCLogstate>();

        return services;
    }

    /// <summary>
    /// Adiciona serviços utilitários específicos do projeto à instância especificada de <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// Este método registra serviços utilitários com tempo de vida scoped.
    /// Deve ser chamado durante a inicialização da aplicação para garantir que os serviços necessários estejam disponíveis para injeção de dependência.
    /// </remarks>
    /// <param name="services">A instância de <see cref="IServiceCollection"/> à qual os serviços utilitários serão adicionados.</param>
    /// <returns>A instância atualizada de <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddProjectUtilities(this IServiceCollection services)
    {
        //services
        //     .AddScoped<SincronizacaoUtils>();
        return services;
    }

    /// <summary>
    /// Adiciona serviços utilizados pelo sistema
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddProjectServices(this IServiceCollection services)
    {
        services
            .AddScoped<AuthService>()
            .AddScoped<IDataSeeder, AdminUserSeeder>();

        //services
        //    .AddScoped<ILoginService, DatabaseLoginService>()
        //    //.AddScoped<ILoginService, AliareAuthService>()
        //    .AddScoped<ILoginService, OIDCLoginService>()
        //    .AddScoped<IConfigurationService, DatabaseConfigurationService>()
        //    .AddScoped<IConfigurationUserService, DatabaseConfigurationUserService>()
        //    .AddScoped<IDeletedRecordService, DeletedRecordService>()
        //    .AddScoped<ITableNameService, TableNameService>()
        //    .AddScoped<IFaixaIdService, FaixaIdService>()
        //    .AddScoped<IDCLogStateService, DCLogStateService>()
        //    .AddScoped<IDCLogStateTableService, DCLogStateTableService>()
        //    .AddScoped<IUserRecordStateService, UserRecordStateService>()
        //    .AddScoped<IDBIdService, DBIdService>()
        //    .AddScoped<IConfiguracaoService, ConfiguracaoService>()
        //    .AddScoped<ISessaoservice, Sessaoservice>()
        //    .AddScoped<IEmailService, EmailService>()
        //    .AddTransient<FileManager>()
        //    .AddTransient<IBackgroundTask, BackgroundFilterWorker>()
        //    .AddSingleton<ISynchronizationTaskManager, SynchronizationTaskManager>()
        //    .AddHostedService<SynchronizationBackgroundService>()
        //    .AddScoped<ITopologicalService, TopologicalService>()
        //    .AddScoped<ISoftDeleteService, SoftDeleteService>()
        //    .AddScoped<IOIDCAuthService, PlataformaAliareAuthService>()
        //    .AddScoped<MobileAccessService>()
        //    .AddScoped<MobileAccessMockProvider>()
        //    .AddScoped(typeof(IAuditChangeService<>), typeof(AuditChangeService<>))
        //    .AddScoped<ILogGeralService, LogGeralService>()
        //    .AddScoped<ILogGeralAtributoService, LogGeralAtributoService>()
        //    .AddScoped<ILogGeralItemDependenteService, LogGeralItemDependenteService>();

        return services;
    }

    /// <summary>
    /// Adiciona serviços genéricos utilizados pelo sistema
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddGenericServices(this IServiceCollection services)
    {
        //services
        //    .AddScoped(typeof(IDataFilterService<>), typeof(DataFilterService<>))
        //    .AddScoped(typeof(ISincronizadorService<>), typeof(SincronizadorService<>))
        //    .AddScoped(typeof(IConflictStrategy<>), typeof(ServerConflictStrategy<>));

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
    /// Toda action exige usuário autenticado, exceto as marcadas com [AllowAnonymous].
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

        services
            .AddMvcCore(options =>
            {
                AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                options.Filters.Add(new AuthorizeFilter(policy));

                options.Filters.Add(new CacheControlHeaderFilter());
            });

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
