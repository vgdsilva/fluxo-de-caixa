namespace WebAPI.Exceptions;

public sealed class SessionExpiredException()
    : AppException(StatusCodes.Status401Unauthorized, "Sessão expirada. Faça login novamente.");
