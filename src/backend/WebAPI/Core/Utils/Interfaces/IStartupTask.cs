namespace WebAPI.Core.Utils.Interfaces;

public interface IStartupTask
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
