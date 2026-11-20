namespace WebAPI.Common.Background;

/// <summary>
/// Tarefa periódica executada pelo ScheduledJobsService (Framework).
/// Cada implementação é registrada no DI e roda em seu próprio escopo.
/// </summary>
public interface IBackgroundTask
{
    /// <summary>Nome usado em logs.</summary>
    string Name { get; }

    /// <summary>Intervalo entre execuções.</summary>
    TimeSpan Interval { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);
}
