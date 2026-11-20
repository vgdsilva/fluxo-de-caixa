namespace WebAPI.Configuration;

/// <summary>Ajustes do host/Kestrel e fontes de configuração.</summary>
public static class BuilderConfiguration
{
    public static WebApplicationBuilder ConfigureBuilder(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables(prefix: "WEBAPI_");
        builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
        return builder;
    }
}
