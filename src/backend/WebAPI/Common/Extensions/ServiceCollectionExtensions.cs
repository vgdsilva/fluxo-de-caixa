using Microsoft.Extensions.FileProviders;

namespace WebAPI.Common.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>Guarda o <see cref="IHttpContextAccessor"/> no <see cref="HttpContextHolder"/> (acesso estático).</summary>
    public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
    {
        var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        HttpContextHolder.Configure(httpContextAccessor);
        return app;
    }

    /// <summary>Tarefa que roda no startup (migrations, seeds).</summary>
    public static IServiceCollection AddStartupTask<T>(this IServiceCollection services) where T : class, IStartupTask =>
        services.AddTransient<IStartupTask, T>();

    public static IServiceCollection AddFileProvider(this IServiceCollection services) =>
        services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
}
