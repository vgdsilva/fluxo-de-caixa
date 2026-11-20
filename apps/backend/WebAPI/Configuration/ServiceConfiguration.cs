using WebAPI.Common.Converters;
using WebAPI.Core;
using WebAPI.Framework.Mvc;
using WebAPI.Framework.Startup;

namespace WebAPI.Configuration;

/// <summary>Registro de serviços no DI.</summary>
public static class ServiceConfiguration
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services
            .AddControllers(options => options.Filters.Add<ValidateModelFilter>())
            .AddJsonOptions(options =>
            {
                foreach (var converter in JsonDefaults.Options.Converters)
                {
                    options.JsonSerializerOptions.Converters.Add(converter);
                }
            });

        services.AddOpenApi();
        services.AddProblemDetails();

        services
            .AddWebAPIData(configuration)
            .AddWebAPIMessaging(typeof(CoreServiceCollectionExtensions).Assembly)
            .AddWebAPICore()
            .AddWebAPIJwtAuthentication(configuration)
            .AddWebAPIBackgroundServices(configuration);

        services.AddCors(options => options.AddDefaultPolicy(policy =>
        {
            var origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        }));

        return builder;
    }
}
