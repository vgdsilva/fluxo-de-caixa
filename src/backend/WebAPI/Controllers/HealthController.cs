using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{

    [HttpGet]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy" });
    }
}
