using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebAPI.Common.Configuration;
using WebAPI.Common.Webhooks;
using WebAPI.Framework.Background;
using WebAPI.Framework.Security;

namespace WebAPI.Framework.Startup;

public static class BackgroundServiceCollectionExtensions
{
    /// <summary>
    /// Registra opções, HttpClient de webhooks e, se Background:Enabled = true,
    /// os quatro BackgroundServices (outbox, fila, webhooks, agendados).
    /// </summary>
    public static IServiceCollection AddWebAPIBackgroundServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BackgroundOptions>(configuration.GetSection(BackgroundOptions.SectionName));
        services.Configure<WebhookOptions>(configuration.GetSection(WebhookOptions.SectionName));

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IWebhookSigner, HmacWebhookSigner>();

        var webhookTimeout = configuration.GetSection(WebhookOptions.SectionName).Get<WebhookOptions>()?.TimeoutSeconds ?? 15;
        services.AddHttpClient(WebhookDeliveryService.HttpClientName, c => c.Timeout = TimeSpan.FromSeconds(webhookTimeout));

        var enabled = configuration.GetSection(BackgroundOptions.SectionName).Get<BackgroundOptions>()?.Enabled ?? true;
        if (enabled)
        {
            services.AddHostedService<OutboxDispatcherService>();
            services.AddHostedService<QueueConsumerService>();
            services.AddHostedService<WebhookDeliveryService>();
            services.AddHostedService<ScheduledJobsService>();
        }

        return services;
    }
}
