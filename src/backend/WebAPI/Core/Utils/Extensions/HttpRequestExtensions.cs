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

        ErrorResponse responseBody = isDevelopment
            ? new ErrorResponse
            {
                Code = (int)statusCode,
                Message = errorMessage,
                Detail = detail,
                StackTrace = stackTrace,
                ReferenceCode = errorId.ToString(),
                DevelopmentNote = "Este detalhe só aparece em ambiente de Development."
            }
            : new ErrorResponse
            {
                Code = (int)statusCode,
                Message = "Ocorreu um erro interno. Por favor, tente novamente mais tarde.",
                ReferenceCode = errorId.ToString()
            };

        string responseJson = JsonSerializer.Serialize(responseBody, JsonDefaults.DefaultConfigs());

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsync(responseJson);
    }
}

/// <summary>
/// Corpo das respostas de erro. É uma classe com setters (e não um tipo anônimo) porque
/// <see cref="JsonDefaults"/> usa IgnoreReadOnlyProperties, que descartaria as propriedades de um tipo anônimo.
/// </summary>
public sealed class ErrorResponse
{
    public int Code { get; set; }

    public string? Message { get; set; }

    public string? Detail { get; set; }

    public string? StackTrace { get; set; }

    public string? ReferenceCode { get; set; }

    public string? DevelopmentNote { get; set; }
}
