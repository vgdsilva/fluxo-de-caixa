using WebAPI.Modules.Auth.Endpoints;

namespace WebAPI.Modules.Auth;

/// <summary>Módulo de autenticação: sessão no banco + JWT de vida curta (equivale ao auth.module.ts do Nest).</summary>
public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AuthSessionOptions>(configuration.GetSection(AuthSessionOptions.SectionName));

        services
            .AddSingleton<TokenManager>()
            .AddScoped<SessionManager>()
            .AddScoped<AuthService>();

        // Esquema que valida o JWT do header Authorization em toda requisição.
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CustomAuthOptions.DefaultScheme;
                options.DefaultChallengeScheme = CustomAuthOptions.DefaultScheme;
            })
            .AddScheme<CustomAuthOptions, CustomAuthHandler>(CustomAuthOptions.DefaultScheme, null);

        services.AddAuthorization();
        services.AddScoped<UserInfo>();

        return services;
    }

    /// <summary>Rotas em /api/auth (equivale ao @Controller('auth')).</summary>
    public static IEndpointRouteBuilder MapAuthModule(this IEndpointRouteBuilder api)
    {
        RouteGroupBuilder group = api.MapGroup("/auth").WithTags("Auth").AllowAnonymous();

        group.MapEndpoint<Login.Endpoint>()
             .MapEndpoint<Refresh.Endpoint>()
             .MapEndpoint<Logout.Endpoint>();

        return api;
    }
}
