using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebAPI.Database;

/// <summary>
/// Usado só pelo <c>dotnet ef</c> (migrations). Lê a mesma seção "Database" do appsettings,
/// os user-secrets e as variáveis de ambiente, sem subir a aplicação.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WebAPIContext>
{
    public WebAPIContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<DesignTimeDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        DatabaseOptions database = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new();

        DbContextOptions<WebAPIContext> options = new DbContextOptionsBuilder<WebAPIContext>()
            .UseNpgsql(database.BuildConnectionString())
            .Options;

        return new WebAPIContext(options);
    }
}
