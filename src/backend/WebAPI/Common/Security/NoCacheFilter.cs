namespace WebAPI.Common.Security;

/// <summary>Respostas da API nunca ficam em cache (navegador ou proxy).</summary>
public sealed class NoCacheFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        IHeaderDictionary headers = context.HttpContext.Response.Headers;
        headers.CacheControl = "no-cache, no-store, must-revalidate";
        headers.Pragma = "no-cache";
        headers.Expires = "0";

        return await next(context);
    }
}
