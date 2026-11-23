using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Scalar.AspNetCore;
using Serilog;
using WebAPI.Core.DependencyServices;
using WebAPI.Core.Utils.Extensions;
using WebAPI.Core.Utils.Middlewares;

namespace WebAPI.Configuration;

/// <summary>
/// Pipeline HTTP. A ordem dos middlewares importa.
/// </summary>
public static class AppConfiguration
{
    /// <summary>
    /// Monta o pipeline e registra os eventos de ciclo de vida.
    /// </summary>
    /// <param name="app"></param>
    public static void ConfigureApp(WebApplication app)
    {
        Log.Information("Inicializando ServiceLocator...");
        ServiceLocator.Initialize(app.Services);

        Log.Information("Preparando acesso estático ao contexto HTTP.");
        app.UseStaticHttpContext();

        Log.Information("Preparando ErrorHandlingMiddleware.");
        app.UseMiddleware<ErrorHandlingMiddleware>();

        Log.Information("Preparanto StartupTasksMiddleware");
        app.UseMiddleware<StartupTasksMiddleware>();

        if (!app.Environment.IsDevelopment())
        {
            Log.Information("Adicionado Hsts.");
            app.UseHsts();
        }

        app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();

            await next();
        });

        Log.Information("Preparando roteamento.");
        app.UseRouting();

        Log.Information("Preparando Rate Limiting.");
        app.UseRateLimiter();

        Log.Information("Preparando autenticação.");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        if (app.Configuration.GetValue("Docs:Enabled", app.Environment.IsDevelopment()))
        {
            Log.Information("Documentação da API em /docs.");
            app.MapOpenApi("/openapi/{documentName}.json");
            app.MapScalarApiReference("/docs", options => options
                .WithTitle("Fluxo de Caixa API")
                .WithOpenApiRoutePattern("/openapi/{documentName}.json"));
        }

        Log.Information("Adicionado eventos de ciclo de vida da aplicação.");
        app.Lifetime.ApplicationStarted.Register(() => _ = RunStartupTasksAsync(app));

        app.Lifetime.ApplicationStopping.Register(() =>
        {
            Log.Information("Parada de aplicação iniciada...");
        });

        app.Lifetime.ApplicationStopped.Register(() =>
        {
            Log.Information("Aplicação finalizada.");

            Log.CloseAndFlush();
        });
    }

    /// <summary>
    /// Roda as tarefas de inicialização (migrations, seeds). Enquanto não terminam, a API responde 503;
    /// se uma delas falhar, a aplicação é encerrada em vez de ficar presa no 503.
    /// </summary>
    private static async Task RunStartupTasksAsync(WebApplication app)
    {
        try
        {
            Log.Information("Executando tarefas de inicialização.");
            await app.RunStartupTasks();

            StartupTasksMiddleware.MarkAsCompleted();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Falha nas tarefas de inicialização. Encerrando a aplicação.");
            app.Lifetime.StopApplication();
            return;
        }

        Log.Information("----");
        if (Debugger.IsAttached)
            Log.Information("");

#if DEBUG
        var server = app.Services.GetService<IServer>();
        var addressesFeature = server?.Features.Get<IServerAddressesFeature>();
        if (addressesFeature != null)
            foreach (var address in addressesFeature.Addresses)
                Log.Information("*** {Address}", address);
#endif

        Log.Information("----");
    }
}
