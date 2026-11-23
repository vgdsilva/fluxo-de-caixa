using Serilog;
using System.Diagnostics;
using WebAPI.Core.Utils.Interfaces;

namespace WebAPI.Core.Utils.Extensions;

public static class StartupTaskWebHostExtensions
{
    public static async Task RunStartupTasks(this IApplicationBuilder app)
    {
        Stopwatch tempo = Stopwatch.StartNew();

        using (IServiceScope serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            IEnumerable<IStartupTask> startupTasks = serviceScope.ServiceProvider.GetServices<IStartupTask>();

            IEnumerable<Task> tasks = startupTasks.Select(s => s.ExecuteAsync());

            await Task.WhenAll(tasks);
        }

        tempo.Stop();

        Log.Information("Tarefas de inicialização executadas com sucesso! {tempo}", $"{tempo.ElapsedMilliseconds}ms");
    }
}
