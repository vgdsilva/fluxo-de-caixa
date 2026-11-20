using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.Common.Exceptions;

namespace WebAPI.Framework.Mvc;

/// <summary>Converte exceções em respostas ProblemDetails (RFC 9457).</summary>
public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted && ex is not OperationCanceledException)
        {
            var problem = ex switch
            {
                ValidationFailedException v => new ValidationProblemDetails(v.Errors.ToDictionary(k => k.Key, k => k.Value))
                {
                    Status = v.StatusCode,
                    Title = v.Message,
                },
                AppException app => new ProblemDetails { Status = app.StatusCode, Title = app.Message },
                _ => new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Title = "Erro interno." },
            };

            if (problem.Status >= 500)
            {
                logger.LogError(ex, "Erro não tratado em {Path}.", context.Request.Path);
            }

            problem.Instance = context.Request.Path;
            problem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = problem.Status ?? 500;
            await context.Response.WriteAsJsonAsync(problem, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json");
        }
    }
}
