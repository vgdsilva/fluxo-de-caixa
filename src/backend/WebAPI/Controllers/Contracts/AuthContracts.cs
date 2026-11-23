using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers.Contracts;

/// <summary>Login por nome de acesso (o e-mail também é aceito) e senha.</summary>
public sealed record LoginRequest(
    [Required, StringLength(254)] string Login,
    [Required, StringLength(200)] string Password);

/// <summary>Corpo de /api/auth/refresh e /api/auth/logout.</summary>
public sealed record RefreshRequest([Required, StringLength(200)] string RefreshToken);

public sealed record UserResponse(Guid Id, string? Name, string? Login, string? Email, string? Role);

/// <summary>
/// Resposta de login e refresh. O <see cref="AccessToken"/> vai no header <c>Authorization: Bearer</c>
/// até <see cref="AccessTokenExpiresAt"/>; antes disso, chame POST /api/auth/refresh com o
/// <see cref="RefreshToken"/>. Cada refresh devolve um refresh token novo e invalida o anterior.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    UserResponse User);
