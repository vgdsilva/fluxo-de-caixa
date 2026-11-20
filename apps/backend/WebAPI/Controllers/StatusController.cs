using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Core.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/status")]
[AllowAnonymous]
public sealed class StatusController(IStatusService statusService, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<StatusInfo>> Get(CancellationToken cancellationToken) =>
        Ok(await statusService.GetAsync(environment.EnvironmentName, cancellationToken));
}
