using WebAPI.Core;
using WebAPI.Framework.Startup;

// Host só de background: registra os mesmos BackgroundServices da WebAPI.
// Ao usar este Worker, defina Background:Enabled = false na WebAPI para não processar em dobro.
// Atenção: com a fila InMemory, a fila não é compartilhada entre processos; troque por RabbitMQ/Service Bus antes.
var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddWebAPIData(builder.Configuration)
    .AddWebAPIMessaging(typeof(CoreServiceCollectionExtensions).Assembly)
    .AddWebAPICore()
    .AddWebAPIBackgroundServices(builder.Configuration);

var host = builder.Build();
await host.RunAsync();
