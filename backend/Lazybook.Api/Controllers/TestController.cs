using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lazybook.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    // GET: api/test/ping
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { message = "pong", timestamp = DateTime.UtcNow });
    }

    // GET: api/test/me
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var claims = HttpContext.User.Claims;
        Console.WriteLine(claims);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;

        return Ok(new { userId, username });
    }
}
