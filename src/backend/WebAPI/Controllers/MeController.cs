using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Contracts;
using WebAPI.Core.Utils.Security;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeController(UserInfo userInfo) : ControllerBase
{
    /// <summary>Usuário do JWT atual.</summary>
    [HttpGet]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public IActionResult Get() => Ok(new UserResponse(userInfo.UserId, userInfo.Name, userInfo.Login, userInfo.Email, userInfo.Role));
}
