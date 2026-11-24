namespace WebAPI.Exceptions;

public sealed class InvalidCredentialsException()
    : AppException(StatusCodes.Status401Unauthorized, "Login ou senha inválidos.");
