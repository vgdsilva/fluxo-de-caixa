using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using WebAPI.Common.Background;
using WebAPI.Common.Messaging;
using WebAPI.Framework.Background;
using WebAPI.Framework.Messaging;

namespace WebAPI.Framework.Startup;

public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Registra a fila InMemory e os tipos de mensagem encontrados nos assemblies informados.
    /// </summary>
    public static IServiceCollection AddWebAPIMessaging(this IServiceCollection services, params Assembly[] messageAssemblies)
    {
        var registry = new MessageTypeRegistry();
        registry.RegisterFromAssemblies([typeof(IMessage).Assembly, .. messageAssemblies]);

        services.AddSingleton(registry);
        services.AddSingleton<MessageDispatcher>();
        services.AddSingleton<InMemoryMessageBus>();
        services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<InMemoryMessageBus>());
        services.AddSingleton<IMessageQueueReader>(sp => sp.GetRequiredService<InMemoryMessageBus>());
        services.AddSingleton(typeof(IJobQueue<>), typeof(ChannelJobQueue<>));
        return services;
    }

    /// <summary>Registra todos os IMessageHandler&lt;T&gt; concretos do assembly como scoped.</summary>
    public static IServiceCollection AddMessageHandlersFrom(this IServiceCollection services, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false }))
        {
            foreach (var iface in type.GetInterfaces()
                         .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>)))
            {
                services.AddScoped(iface, type);
            }
        }

        return services;
    }
}
