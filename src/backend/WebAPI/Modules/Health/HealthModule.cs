using WebAPI.Modules.Health.Endpoints;

namespace WebAPI.Modules.Health;

public static class HealthModule
{
    /// <summary>Rotas em /api/health.</summary>
    public static IEndpointRouteBuilder MapHealthModule(this IEndpointRouteBuilder api)
    {
        RouteGroupBuilder group = api.MapGroup("/health").WithTags("Health").AllowAnonymous();

        group.MapEndpoint<GetHealth.Endpoint>();

        return api;
    }
}
