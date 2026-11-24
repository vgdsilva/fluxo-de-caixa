namespace WebAPI.Common.Middleware;

public class StartupTasksMiddleware
{
    private static bool _startupCompleted = false;
    private readonly RequestDelegate _next;

    public StartupTasksMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public static void MarkAsCompleted()
    {
        _startupCompleted = true;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_startupCompleted)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return;
        }

        await _next.Invoke(context);
    }
}
