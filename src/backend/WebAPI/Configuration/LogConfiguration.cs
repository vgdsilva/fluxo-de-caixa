using Serilog;

namespace WebAPI.Configuration;

/// <summary>
/// Configura o sistema de log usando as configurações do arquivo de configuração da aplicação.
/// </summary>
/// <remarks>Este método lê as configurações de log do arquivo "appsettings.json" 
/// localizado no diretório atual da aplicação. A configuração é aplicada ao sistema de log 
/// usando Serilog. Certifique-se de que o arquivo "appsettings.json" contém as seções de 
/// configuração de log necessárias.</remarks>
public static class LogConfiguration
{
    /// <summary>
    /// Configura o sistema de log usando as configurações do arquivo "appsettings.json".
    /// </summary>
    /// <remarks>Este método inicializa o sistema de log lendo as configurações do arquivo 
    /// "appsettings.json" localizado no diretório atual da aplicação. A configuração do log 
    /// é aplicada imediatamente e suporta recarregamento automático caso o arquivo 
    /// seja alterado.</remarks>
    public static void ConfigureLog()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Destructure.ByTransforming<HttpRequest>(static req => new
            {
                req.Method,
                req.Path,
                req.QueryString,
                Headers = req.Headers.Where(h =>
                    !h.Key.Contains("authorization", StringComparison.CurrentCultureIgnoreCase) &&
                    !h.Key.Contains("cookie", StringComparison.CurrentCultureIgnoreCase) &&
                    !h.Key.Contains("token", StringComparison.CurrentCultureIgnoreCase) &&
                    !h.Key.Contains("senha", StringComparison.CurrentCultureIgnoreCase) &&
                    !h.Key.Contains("password", StringComparison.CurrentCultureIgnoreCase))
            })
            .CreateLogger();
    }
}
