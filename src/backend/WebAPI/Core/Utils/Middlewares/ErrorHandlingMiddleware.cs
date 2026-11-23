using Serilog;
using WebAPI.Core.Utils.Extensions;

namespace WebAPI.Core.Utils.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next.Invoke(context);

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized ||
                context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                var ip = context.Connection.RemoteIpAddress?.ToString();
                Log.Warning("Security Event: Access Denied at {Path} | IP: {IP} | Status: {Status}",
                    context.Request.Path, ip, context.Response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            string errorId = Guid.NewGuid().ToString(); //.Substring(0, 8).ToUpper();
            string? ip = context.Connection.RemoteIpAddress?.ToString();

            Log.Error(ex, "Unhandled Exception [{ErrorId}] at {Path} | IP: {IP} | Message: {Message}",
                errorId, context.Request.Path, ip, ex.GetBaseException().Message);

            if (context.Request.Headers.TryGetValue("DadosErroHeader", out var value))
                Log.Error("Client Error Context [{ErrorId}]: {ClientData}", errorId, value);

            context.Response.Headers["X-Reference-ID"] = errorId;

            await context.HandleErrorAsync(ex);
        }
    }
}
