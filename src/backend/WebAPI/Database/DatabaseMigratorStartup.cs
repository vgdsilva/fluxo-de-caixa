using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using System.Data;
using System.Diagnostics;

namespace WebAPI.Database;

public class DatabaseMigratorStartup : IStartupTask
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseMigratorStartup(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        Log.Information("Iniciando contexto de banco de dados...");

        Stopwatch tempo = Stopwatch.StartNew();

        using IServiceScope scope = _serviceProvider.CreateScope();
        WebAPIContext dbContext = scope.ServiceProvider.GetRequiredService<WebAPIContext>();

        try
        {
            // 1. Verifica se o banco existe
            var creator = dbContext.GetService<IRelationalDatabaseCreator>();
            bool existe = await creator.ExistsAsync(cancellationToken);

            if (!existe)
            {
                Log.Warning("[DatabaseMigratorStartup] Banco de dados não existe. Será criado junto com as migrations.");
                await creator.CreateAsync(cancellationToken);
            }

            // 2. Aplica as migrations pendentes
            var pendentes = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

            if (pendentes.Count > 0)
            {
                Log.Information("[DatabaseMigratorStartup] Aplicando {qtd} migration(s): {migrations}", pendentes.Count, pendentes);
                await dbContext.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                Log.Information("[DatabaseMigratorStartup] Nenhuma migration pendente.");
            }

            // 3. Seeds dos dados iniciais
            var seeders = scope.ServiceProvider.GetServices<IDataSeeder>().OrderBy(s => s.Order);

            foreach (var seeder in seeders)
            {
                Log.Information("[DatabaseMigratorStartup] Executando seed {seed}...", seeder.GetType().Name);
                await seeder.SeedAsync(dbContext, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "[DatabaseMigratorStartup] Falha ao inicializar o banco de dados.");
            throw;
        }

        tempo.Stop();
        Log.Information("Contexto de banco de dados inicializado com sucesso! {tempo}ms", tempo.ElapsedMilliseconds);
    }
}