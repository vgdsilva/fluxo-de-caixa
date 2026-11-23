using Microsoft.AspNetCore.Http.Extensions;
using Serilog;
using System.Net;
using System.Text.Json;
using WebAPI.Core.Utils.Classes;

namespace WebAPI.Core.Utils.Extensions;

public static class HttpRequestExtensions
{
    public static async Task HandleErrorAsync(this HttpContext httpContext,
                                              Exception exception,
                                              HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        var path = UriHelper.GetEncodedUrl(httpContext.Request);
        var method = httpContext.Request.Method;

        var errorMessage = exception.GetBaseException().Message;
        string detail = string.Empty;

        Log.Error(exception, "Erro ao processar requisição {Method} para o endereço: {Path}.", method, path);

        if (exception.InnerException is Npgsql.PostgresException postgresException)
        {
            detail = $"{postgresException.SqlState}: {postgresException.MessageText} {postgresException.Detail}".Trim();
        }


        await HandleErrorAsync(httpContext, errorMessage, detail, exception.StackTrace ?? string.Empty, statusCode);
    }

    public static async Task HandleErrorAsync(this HttpContext httpContext,
                                                  string errorMessage,
                                                  HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        await HandleErrorAsync(httpContext, errorMessage, string.Empty, string.Empty, statusCode);
    }

    private static async Task HandleErrorAsync(this HttpContext httpContext,
                                               string errorMessage,
                                               string detail,
                                               string stackTrace,
                                               HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        var env = httpContext.RequestServices.GetService<IHostEnvironment>();
        bool isDevelopment = env?.IsDevelopment() ?? false;

        if (!httpContext.Response.Headers.TryGetValue("X-Reference-ID", out var errorId))
        {
            errorId = Guid.NewGuid().ToString(); //.Substring(0, 8).ToUpper();
        }

        var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = (int)statusCode,
            Title = isDevelopment ? errorMessage : "Ocorreu um erro interno. Por favor, tente novamente mais tarde.",
            Detail = isDevelopment && !string.IsNullOrEmpty(detail) ? detail : null,
        };
        problem.Extensions["referenceCode"] = errorId.ToString();
        if (isDevelopment && !string.IsNullOrEmpty(stackTrace))
            problem.Extensions["stackTrace"] = stackTrace;

        httpContext.Response.StatusCode = (int)statusCode;

        IProblemDetailsService? problemDetails = httpContext.RequestServices.GetService<IProblemDetailsService>();
        if (problemDetails is not null &&
            await problemDetails.TryWriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = problem }))
            return;

        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonDefaults.DefaultConfigs()));
    }
}
