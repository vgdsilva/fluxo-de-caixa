namespace WebAPI;

/// <summary>
/// Tipo âncora para WebApplicationFactory&lt;T&gt; nos testes. Usado no lugar de "Program"
/// porque WebAPI e Worker têm, cada um, uma classe Program no namespace global.
/// </summary>
public sealed class ApiAssemblyMarker;
