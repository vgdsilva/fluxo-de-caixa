using Serilog;

namespace WebAPI.Middlewares;

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
        catch (AppException ex)
        {
            // Erro de negócio que já traz o status (como o @ResponseStatus do Spring): vira ProblemDetails
            // com a mensagem da exceção, sem log de erro.
            Log.Information("{Exception} em {Path}: {Message}", ex.GetType().Name, context.Request.Path, ex.Message);
            context.Response.StatusCode = ex.StatusCode;
            await context.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails { Status = ex.StatusCode, Title = ex.Message },
            });
        }
        catch (BadHttpRequestException ex)
        {
            // JSON malformado ou corpo ausente num endpoint de minimal API: é erro do cliente (400), não 500.
            Log.Warning("Requisição inválida em {Path}: {Message}", context.Request.Path, ex.Message);
            await context.HandleErrorAsync(ex, (System.Net.HttpStatusCode)ex.StatusCode);
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
