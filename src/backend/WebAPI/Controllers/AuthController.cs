using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebAPI.Controllers.Contracts;
using WebAPI.Core.Services;

namespace WebAPI.Controllers;

/// <summary>
/// Login com sessão no banco + JWT de vida curta. O login devolve o JWT e o refresh token da sessão;
/// o refresh troca o refresh token por um novo (rotação) e emite outro JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController(AuthService authService) : ControllerBase
{
    /// <summary>Login por nome de acesso (ou e-mail) e senha.</summary>
    [HttpPost("login")]
    [EnableRateLimiting("AuthRateLimiter")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        AuthResult? result = await authService.LoginAsync(
            request.Login,
            request.Password,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        if (result is null)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Login ou senha inválidos.");

        return Ok(ToResponse(result));
    }

    /// <summary>Troca o refresh token por um novo e emite outro JWT.</summary>
    [HttpPost("refresh")]
    [EnableRateLimiting("AuthRateLimiter")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        AuthResult? result = await authService.RefreshAsync(request.RefreshToken, cancellationToken);

        if (result is null)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Sessão expirada. Faça login novamente.");

        return Ok(ToResponse(result));
    }

    /// <summary>Encerra a sessão do refresh token. Responde 204 mesmo se a sessão já não existir.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    private static AuthResponse ToResponse(AuthResult result) => new(
        result.AccessToken,
        result.AccessTokenExpiresAt,
        result.RefreshToken,
        result.RefreshTokenExpiresAt,
        new UserResponse(result.User.Id, result.User.Name, result.User.Login, result.User.Email, result.User.Role.ToString()));
}
