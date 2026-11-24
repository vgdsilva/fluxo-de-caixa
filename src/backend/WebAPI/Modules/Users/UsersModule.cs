using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebAPI.Modules.Users.Endpoints;

namespace WebAPI.Modules.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AdminUserOptions>(configuration.GetSection(AdminUserOptions.SectionName));

        services.TryAddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IDataSeeder, AdminUserSeeder>();

        return services;
    }

    /// <summary>Rotas em /api/me.</summary>
    public static IEndpointRouteBuilder MapUsersModule(this IEndpointRouteBuilder api)
    {
        RouteGroupBuilder group = api.MapGroup("/me").WithTags("Me");

        group.MapEndpoint<GetMe.Endpoint>();

        return api;
    }
}
