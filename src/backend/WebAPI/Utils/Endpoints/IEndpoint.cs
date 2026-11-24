namespace WebAPI.Utils.Endpoints;

/// <summary>
/// Um endpoint de minimal API. Cada arquivo de endpoint é uma classe estática com as subclasses
/// <c>Request</c>, <c>Response</c>, <c>Validation</c> e <c>Endpoint</c>; a <c>Endpoint</c> implementa esta interface.
/// </summary>
public interface IEndpoint
{
    /// <summary>Registra a rota no grupo <c>/api</c> (ex.: <c>api.MapPost("/auth/login", Handle)</c>).</summary>
    void Map(IEndpointRouteBuilder api);
}
