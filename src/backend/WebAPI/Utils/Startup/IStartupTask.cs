namespace WebAPI.Utils.Startup;

public interface IStartupTask
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
