using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FluentValidation;
using Serilog;

namespace WebAPI.Config;

/// <summary>
/// Registro de serviços no DI: o que é de todos aqui, e cada módulo pelo seu Add*Module
/// (como o <c>imports</c> do AppModule no Nest).
/// </summary>
public static class ServiceConfiguration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        ConfigurationManager configuration = builder.Configuration;

        Log.Information("Iniciando configuração de serviços...");

        // JSON das minimal APIs: camelCase, enums como texto, nulos omitidos.
        services.ConfigureHttpJsonOptions(options =>
        {
            JsonSerializerOptions json = options.SerializerOptions;
            json.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            json.PropertyNameCaseInsensitive = true;
            json.WriteIndented = builder.Environment.IsDevelopment();
            json.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            json.Converters.Add(new JsonStringEnumConverter());
            json.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            json.MaxDepth = 64;
        });

        // Erros saem como ProblemDetails (RFC 9457): { type, title, status, detail, traceId }.
        services.AddProblemDetails();

        Log.Information("Configurando documentação da API (OpenAPI + Scalar)...");
        services.AddOpenApi(options => options.UseNestedTypeNames());

        // Validators: as classes Validation de cada endpoint. Erros de validação em camelCase, como o JSON.
        services.AddValidatorsFromAssemblyContaining<Program>();
        ValidatorOptions.Global.PropertyNameResolver = (_, member, _) =>
            member is null ? null : JsonNamingPolicy.CamelCase.ConvertName(member.Name);
        ValidatorOptions.Global.LanguageManager.Culture = new System.Globalization.CultureInfo("pt-BR");

        // Sem CORS: o front é servido na mesma origem da API (em dev, o Vite repassa /api).

        Log.Information("Configurando Rate Limiting para autenticação...");
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
                new ValueTask(context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>()
                    .WriteAsync(new ProblemDetailsContext
                    {
                        HttpContext = context.HttpContext,
                        ProblemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                        {
                            Status = StatusCodes.Status429TooManyRequests,
                            Title = "Muitas tentativas. Aguarde um minuto e tente de novo.",
                        },
                    }).AsTask());
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

        services.AddHttpContextAccessor();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IObjectMapper, ObjectMapper>();
        services.AddFileProvider();
        services.AddHttpClient();

        Log.Information("Registrando módulos...");
        services.AddDatabaseModule(configuration);
        services.AddUsersModule(configuration);
        services.AddAuthModule(configuration);

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
    }
}
