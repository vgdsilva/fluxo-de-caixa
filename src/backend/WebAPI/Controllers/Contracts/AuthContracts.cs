using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers.Contracts;

public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(200)] string Password);

public sealed record UserResponse(Guid Id, string? Name, string? Email, string? Role);

/// <summary>
/// Resposta de login e refresh. O <see cref="AccessToken"/> vai no header
/// <c>Authorization: Bearer</c>; antes de <see cref="ExpiresAt"/>, chame POST /api/auth/refresh
/// (o token da sessão viaja sozinho no cookie HttpOnly).
/// </summary>
public sealed record AuthResponse(string AccessToken, DateTime ExpiresAt, UserResponse User);
