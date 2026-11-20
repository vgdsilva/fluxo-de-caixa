namespace WebAPI.Core.Services;

public sealed record StatusInfo(string Application, string Version, string Environment, bool DatabaseOk, DateTimeOffset ServerTime);

/// <summary>Exemplo de serviço de caso de uso (usado pelo StatusController).</summary>
public interface IStatusService
{
    Task<StatusInfo> GetAsync(string environment, CancellationToken cancellationToken = default);
}
