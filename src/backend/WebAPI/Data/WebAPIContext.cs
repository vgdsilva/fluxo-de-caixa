using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace WebAPI.Data;

/// <summary>
/// Toda a configuração do EF fica aqui: conexão (a partir de <see cref="DatabaseOptions"/>),
/// interceptor de auditoria, tracking e mapeamento.
/// </summary>
/// <remarks>
/// Por configurar tudo no <see cref="OnConfiguring"/>, o contexto não usa pooling
/// (AddPooledDbContextFactory proíbe alterar as opções no OnConfiguring).
/// </remarks>
public class WebAPIContext : DbContext
{
    private readonly DatabaseOptions? _database;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public WebAPIContext(DbContextOptions<WebAPIContext> options,
                         IOptions<DatabaseOptions>? database = null,
                         IHttpContextAccessor? httpContextAccessor = null) : base(options)
    {
        _database = database?.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Quem cria o contexto já com conexão (testes, design-time) passa as opções prontas.
        if (!optionsBuilder.IsConfigured)
        {
            if (_database is null)
                throw new InvalidOperationException("Configuração 'Database' não encontrada.");

            optionsBuilder.UseNpgsql(_database.BuildConnectionString());

            if (_database.EnableSensitiveDataLogging)
            {
                optionsBuilder
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging();
            }
        }

        optionsBuilder
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .EnableThreadSafetyChecks()
            .AddInterceptors(new AuditInterceptor(_httpContextAccessor));

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("public");

        builder.AddEntities();

        builder.Entity<AuditLog>()
            .Property(a => a.Changes)
            .HasConversion(new AuditLogChangesConverter());

        base.OnModelCreating(builder);
    }
}
