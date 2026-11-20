using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebAPI.Common.Background;

namespace WebAPI.Framework.Background;

/// <summary>
/// Executa cada IBackgroundTask registrado no seu próprio intervalo
/// (ex.: gerar lançamentos recorrentes, limpar outbox antigo).
/// </summary>
public sealed class ScheduledJobsService(
    IServiceScopeFactory scopeFactory,
    ILogger<ScheduledJobsService> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<(string Name, TimeSpan Interval, Type Type)> tasks;
        using (var scope = scopeFactory.CreateScope())
        {
            tasks = scope.ServiceProvider.GetServices<IBackgroundTask>()
                .Select(t => (t.Name, t.Interval, t.GetType()))
                .ToList();
        }

        if (tasks.Count == 0)
        {
            logger.LogInformation("Nenhuma tarefa agendada registrada.");
            return Task.CompletedTask;
        }

        return Task.WhenAll(tasks.Select(t => RunLoopAsync(t.Name, t.Interval, t.Type, stoppingToken)));
    }

    private async Task RunLoopAsync(string name, TimeSpan interval, Type taskType, CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var task = scope.ServiceProvider.GetServices<IBackgroundTask>().First(t => t.GetType() == taskType);
                await task.ExecuteAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falha na tarefa agendada {Task}.", name);
            }
        }
    }
}
