using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using WebAPI.Core.Utils.Interfaces;
using WebAPI.Core.Utils.ObjectMappers;
using WebAPI.Core.Utils.Startup;

namespace WebAPI.Configuration;

/// <summary>
/// Registro de serviços no DI.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Registra controllers, banco, autenticação, documentação e tarefas de startup.
    /// </summary>
    /// <param name="builder"></param>
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        ConfigurationManager configuration = builder.Configuration;

        Log.Information("Iniciando configuração de serviços...");

        Log.Information("Adicionando controladores...");
        services
            .AddControllers(options =>
            {
                options.ReturnHttpNotAcceptable = true;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();

                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.MaxDepth = 64;
                options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
            });

        Log.Information("Configurando documentação da API (OpenAPI + Scalar)...");
        services.AddOpenApi();

        services.AddOptions();

        // Sem CORS: o front é servido na mesma origem da API (em dev, o Vite repassa /api).

        Log.Information("Configurando Rate Limiting para autenticação...");
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("AuthRateLimiter", httpContext =>
            {
                IPAddress clientIp = httpContext.Connection.RemoteIpAddress ?? IPAddress.Loopback;
                return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });
        });

        services.AddModels();

        Log.Information("Configurando accessor de contexto HTTP...");
        services.AddHttpContextAccessor();

        Log.Information("Configurando contexto banco de dados...");
        services.AddProjectDatabaseContext(configuration);

        Log.Information("Configurando services...");
        services.AddProjectServices();
        services.AddGenericServices();

        services.AddProjectUtilities();

        Log.Information("Configurando mecanismo de autenticação...");
        services.AddCustomAuthentication(configuration);

        Log.Information("Configurando ObjectMapper...");
        services.AddSingleton<IObjectMapper, ObjectMapper>();

        Log.Information("Configurando FileProvider...");
        services.AddFileProvider();

        Log.Information("Adicionando tarefas de execução em tempo de inicialização...");
        services.AddStartupTask<DatabaseMigratorStartup>()
                .TryAddSingleton(services);

        services.AddHttpClient();

        if (!builder.Environment.IsDevelopment())
        {
            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
            });
        }

        // O ServiceLocator é inicializado em AppConfiguration, com o provider final da aplicação
        // (app.Services). Montar um provider aqui com BuildServiceProvider() criaria um segundo container.
        Log.Information("Finalizando configuração de injeção de dependências...");
    }
}
