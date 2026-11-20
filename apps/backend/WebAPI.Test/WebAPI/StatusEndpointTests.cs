using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WebAPI.Test.WebAPI;

public sealed class StatusEndpointTests : IClassFixture<StatusEndpointTests.Factory>
{
    private readonly Factory _factory;

    public StatusEndpointTests(Factory factory) => _factory = factory;

    [Fact]
    public async Task Status_responde_200()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/status", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Endpoint_protegido_exige_token()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/webhooks/subscriptions", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public sealed class Factory : WebApplicationFactory<global::WebAPI.ApiAssemblyMarker>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // "Testing": não aplica migrations no start nem liga os BackgroundServices.
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Default",
                Environment.GetEnvironmentVariable(Data.PostgresFixture.EnvironmentVariable)
                ?? "Host=127.0.0.1;Port=1;Database=inexistente;Username=x;Password=x;Timeout=1");
            builder.UseSetting("Background:Enabled", "false");
            builder.UseSetting("Jwt:SigningKey", "chave-de-teste-com-mais-de-32-caracteres-123");
        }
    }
}
