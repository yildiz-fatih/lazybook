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
}
