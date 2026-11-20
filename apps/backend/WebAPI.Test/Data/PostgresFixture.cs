using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebAPI.Data;

namespace WebAPI.Test.Data;

/// <summary>
/// Cria um banco PostgreSQL temporário por classe de teste e apaga no final.
/// Requer a variável WEBAPI_TEST_POSTGRES com uma connection string do servidor (sem Database); ver README.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    public const string EnvironmentVariable = "WEBAPI_TEST_POSTGRES";

    private readonly string? _baseConnectionString = Environment.GetEnvironmentVariable(EnvironmentVariable);

    public bool IsAvailable => !string.IsNullOrWhiteSpace(_baseConnectionString);

    public string ConnectionString { get; private set; } = string.Empty;

    public WebAPIContext CreateContext() =>
        new(new DbContextOptionsBuilder<WebAPIContext>().UseNpgsql(ConnectionString).Options);

    public async ValueTask InitializeAsync()
    {
        if (!IsAvailable)
        {
            return;
        }

        ConnectionString = new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            Database = $"webapi_test_{Guid.NewGuid():N}",
        }.ConnectionString;

        await using var db = CreateContext();
        await db.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (!IsAvailable)
        {
            return;
        }

        await using var db = CreateContext();
        await db.Database.EnsureDeletedAsync();
    }
}
