using Serilog;

namespace WebAPI.Configuration;

/// <summary>
/// Criação do builder: configuração, Serilog e Kestrel.
/// </summary>
public static class BuilderConfiguration
{
    /// <summary>
    /// Cria o <see cref="WebApplicationBuilder"/> com Serilog e as opções de Kestrel do appsettings.
    /// Os limites do Kestrel ficam nos padrões do ASP.NET Core, a não ser que a seção "Kestrel" os altere.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static WebApplicationBuilder CreateBuilder(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSerilog((services, configuration) => configuration
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext());

        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            options.Configure(context.Configuration.GetSection("Kestrel"));
            options.AddServerHeader = false;
        });

        return builder;
    }
}
