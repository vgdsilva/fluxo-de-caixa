
using Serilog;
using WebAPI.Configuration;

namespace WebAPI;

/// <summary>
/// 
/// </summary>
public class Program
{
    private static async Task Main(string[] args)
    {
        LogConfiguration.ConfigureLog();

        AjustarThreadPool();

        WebApplicationBuilder builder = BuilderConfiguration.CreateBuilder(args);

        ServiceConfiguration.ConfigureServices(builder);

        WebApplication app = builder.Build();

        AppConfiguration.ConfigureApp(app);

        await app.RunAsync();
    }

    private static void AjustarThreadPool()
    {
        int processorCounter = Environment.ProcessorCount;
        int minThread = 16 * processorCounter;
        bool success = ThreadPool.SetMinThreads(minThread, minThread);
        if (!success)
            Log.Error("Erro: Não foi possível ajustar o número de threads adequado para execução do sistema.");
    }
}
