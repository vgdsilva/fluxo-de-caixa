namespace WebAPI.Common.Endpoints;

/// <summary>
/// Um endpoint de minimal API. Cada arquivo de endpoint é uma classe estática com as subclasses
/// <c>Request</c>, <c>Response</c>, <c>Validation</c> e <c>Endpoint</c>; a <c>Endpoint</c> implementa esta interface.
/// </summary>
public interface IEndpoint
{
    /// <summary>Registra a rota no grupo do módulo (ex.: <c>group.MapPost("/login", Handle)</c>).</summary>
    void Map(IEndpointRouteBuilder group);
}
