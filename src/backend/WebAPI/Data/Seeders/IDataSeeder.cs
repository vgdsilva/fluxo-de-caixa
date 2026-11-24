
namespace WebAPI.Data.Seeders;

public interface IDataSeeder
{
    /// Define a ordem de execução (ex.: perfis antes de usuários).
    int Order { get; }

    Task SeedAsync(WebAPIContext context, CancellationToken cancellationToken = default);
}
