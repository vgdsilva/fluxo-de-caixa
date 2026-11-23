namespace WebAPI.Core.DependencyServices;

/// <summary>
/// Acesso estático ao DI para pontos onde não há injeção por construtor.
/// Prefira construtor sempre que puder.
/// </summary>
/// <remarks>
/// <para>Inicializado em AppConfiguration com <c>app.Services</c> (o mesmo container da aplicação).</para>
/// <para>Durante uma requisição, <see cref="Current"/> usa os serviços da requisição, então serviços
/// scoped (ex.: WebAPIContext) são os mesmos que os controllers recebem.</para>
/// <para>Fora de uma requisição (startup, background), serviços scoped só podem ser obtidos dentro de
/// um escopo: <c>using var scope = ServiceLocator.Current.CreateScope();</c></para>
/// </remarks>
public class ServiceLocator
{
    private static IServiceProvider? _rootProvider;

    private readonly IServiceProvider _currentServiceProvider;

    public ServiceLocator(IServiceProvider currentServiceProvider)
    {
        _currentServiceProvider = currentServiceProvider;
    }

    public static bool IsInitialized => _rootProvider is not null;

    public static ServiceLocator Current
    {
        get
        {
            IServiceProvider root = _rootProvider
                ?? throw new InvalidOperationException("ServiceLocator não foi inicializado.");

            HttpContext? httpContext = root.GetService<IHttpContextAccessor>()?.HttpContext;

            return new ServiceLocator(httpContext?.RequestServices ?? root);
        }
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _rootProvider = serviceProvider;
    }

    public object? GetService(Type serviceType) => _currentServiceProvider.GetService(serviceType);

    public TService? GetService<TService>() => _currentServiceProvider.GetService<TService>();

    public object GetRequiredService(Type serviceType) => _currentServiceProvider.GetRequiredService(serviceType);

    public TService GetRequiredService<TService>() where TService : notnull =>
        _currentServiceProvider.GetRequiredService<TService>();

    public IServiceScope CreateScope() => _currentServiceProvider.CreateScope();
}
