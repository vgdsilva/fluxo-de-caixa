using WebAPI.Data;

namespace WebAPI.Core.Services;

public sealed class StatusService(WebAPIContext db, TimeProvider timeProvider) : IStatusService
{
    public async Task<StatusInfo> GetAsync(string environment, CancellationToken cancellationToken = default)
    {
        var assembly = typeof(StatusService).Assembly.GetName();
        var databaseOk = await db.Database.CanConnectAsync(cancellationToken);
        return new StatusInfo("WebAPI", assembly.Version?.ToString() ?? "0.0.0", environment, databaseOk, timeProvider.GetUtcNow());
    }
}
