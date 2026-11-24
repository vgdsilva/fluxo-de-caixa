using Microsoft.AspNetCore.Http.HttpResults;

namespace WebAPI.Modules.Health.Endpoints;

/// <summary>GET /api/health: a API está de pé (e as tarefas de startup terminaram; antes disso responde 503).</summary>
public static class GetHealth
{
    public sealed record Response(string Status);

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/", Handle).WithName("GetHealth");

        private static Ok<Response> Handle() => TypedResults.Ok(new Response("Healthy"));
    }
}
