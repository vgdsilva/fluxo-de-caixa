using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Common.Authentication;

namespace WebAPI.Controllers;

public sealed record LoginRequest(string Email, string Password);

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ICurrentUser currentUser) : ControllerBase
{
    /// <summary>
    /// TODO: validar credenciais quando a entidade de usuário existir (hash com PasswordHasher/Argon2)
    /// e então emitir o token com ITokenService.CreateToken(...).
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login(LoginRequest request) =>
        Problem(statusCode: StatusCodes.Status501NotImplemented, title: "Login ainda não implementado.");

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new { currentUser.UserId, currentUser.Name });
}
