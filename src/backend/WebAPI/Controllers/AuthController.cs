using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebAPI.Controllers.Contracts;
using WebAPI.Core.Services;
using WebAPI.Core.Utils.Security;

namespace WebAPI.Controllers;

/// <summary>
/// Login com sessão no banco + JWT de vida curta.
/// O token da sessão fica num cookie HttpOnly restrito a /api/auth; o JWT vai no corpo da resposta.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController(AuthService authService, SessionManager sessionManager) : ControllerBase
{
    /// <summary>Login por e-mail e senha.</summary>
    [HttpPost("login")]
    [EnableRateLimiting("AuthRateLimiter")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        AuthResult? result = await authService.LoginAsync(
            request.Email,
            request.Password,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        if (result is null)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "E-mail ou senha inválidos.");

        return Ok(Respond(result));
    }

    /// <summary>Renova o JWT usando a sessão do cookie (e troca o token da sessão).</summary>
    [HttpPost("refresh")]
    [EnableRateLimiting("AuthRateLimiter")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        AuthResult? result = null;

        if (Request.Cookies.TryGetValue(sessionManager.CookieName, out string? token) && !string.IsNullOrEmpty(token))
            result = await authService.RefreshAsync(token, cancellationToken);

        if (result is null)
        {
            DeleteCookie();
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Sessão expirada. Faça login novamente.");
        }

        return Ok(Respond(result));
    }

    /// <summary>Encerra a sessão atual e apaga o cookie.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(sessionManager.CookieName, out string? token) && !string.IsNullOrEmpty(token))
            await authService.LogoutAsync(token, cancellationToken);

        DeleteCookie();
        return NoContent();
    }

    private AuthResponse Respond(AuthResult result)
    {
        Response.Cookies.Append(sessionManager.CookieName, result.RefreshToken,
            sessionManager.CreateCookieOptions(result.RefreshTokenExpiresAt));

        return new AuthResponse(
            result.AccessToken,
            result.AccessTokenExpiresAt,
            new UserResponse(result.User.Id, result.User.Name, result.User.Email, result.User.Role.ToString()));
    }

    private void DeleteCookie() =>
        Response.Cookies.Delete(sessionManager.CookieName, sessionManager.CreateCookieOptions(DateTime.UtcNow));
}
