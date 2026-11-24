namespace WebAPI.Common.Startup;

public interface IStartupTask
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
