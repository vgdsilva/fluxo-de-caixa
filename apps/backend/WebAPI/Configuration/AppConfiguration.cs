using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Framework.Mvc;
using WebAPI.Middleware;

namespace WebAPI.Configuration;

/// <summary>Pipeline HTTP (ordem dos middlewares importa).</summary>
public static class AppConfiguration
{
    public static WebApplication ConfigureApp(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ErrorHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            ApplyMigrations(app);
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }

    /// <summary>
    /// Em desenvolvimento, cria/atualiza o banco PostgreSQL automaticamente.
    /// Enquanto não houver migrations, usa EnsureCreated (depois de criar a primeira migration, passa a usar Migrate).
    /// </summary>
    private static void ApplyMigrations(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WebAPIContext>();
        if (db.Database.GetMigrations().Any())
        {
            db.Database.Migrate();
        }
        else
        {
            db.Database.EnsureCreated();
        }
    }
}
