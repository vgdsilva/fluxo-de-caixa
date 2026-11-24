namespace WebAPI.Exceptions;

/// <summary>
/// Erro de negócio que já sabe o status HTTP (como o <c>@ResponseStatus</c> do Spring).
/// O <see cref="Middlewares.ErrorHandlingMiddleware"/> transforma em ProblemDetails; o controller não precisa de try/catch.
/// </summary>
public abstract class AppException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
