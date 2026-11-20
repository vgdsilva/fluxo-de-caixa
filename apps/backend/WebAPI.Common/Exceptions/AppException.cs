namespace WebAPI.Common.Exceptions;

/// <summary>Base das exceções de negócio. O ErrorHandlingMiddleware converte em ProblemDetails.</summary>
public abstract class AppException(string message, int statusCode, Exception? inner = null)
    : Exception(message, inner)
{
    public int StatusCode { get; } = statusCode;
}

public sealed class NotFoundException(string message) : AppException(message, 404);

public sealed class ConflictException(string message) : AppException(message, 409);

public sealed class BusinessRuleException(string message) : AppException(message, 422);

public sealed class ValidationFailedException(IReadOnlyDictionary<string, string[]> errors)
    : AppException("Um ou mais campos são inválidos.", 400)
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
