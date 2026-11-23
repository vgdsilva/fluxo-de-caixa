namespace WebAPI.Core.Utils.Extensions;

public static class HttpContextHolder
{
    private static IHttpContextAccessor? _contextAccessor;

    public static HttpContext? Current => _contextAccessor?.HttpContext;

    public static void Configure(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }
}
