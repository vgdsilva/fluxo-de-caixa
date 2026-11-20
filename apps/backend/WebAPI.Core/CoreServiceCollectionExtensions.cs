using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebAPI.Common.Webhooks;
using WebAPI.Core.Services;
using WebAPI.Core.Webhooks;
using WebAPI.Framework.Startup;

namespace WebAPI.Core;

/// <summary>
/// Registro de DI do Core. Fica aqui (e não em Framework/Startup) porque Framework não referencia Core.
/// </summary>
public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddWebAPICore(this IServiceCollection services)
    {
        var assembly = typeof(CoreServiceCollectionExtensions).Assembly;

        services.TryAddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        services.AddMessageHandlersFrom(assembly);

        services.AddScoped<IStatusService, StatusService>();
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IWebhookDispatcher, WebhookDispatcher>();
        services.AddSingleton<IWebhookRetryPolicy, ExponentialBackoffRetryPolicy>();
        return services;
    }
}
