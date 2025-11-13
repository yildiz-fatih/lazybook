using Lazybook.Api.Data;
using Lazybook.Api.DTOs;
using Lazybook.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lazybook.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly AuthService _authService;

        public AuthController(AppDbContext dbContext, AuthService authService)
        {
            _dbContext = dbContext;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Check database if user exists already
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (existingUser != null)
            {
                return Conflict(new { message = "Username already taken" });
            }
            // Create new user entity
            var newUser = new Entities.User
            {
                Username = request.Username,
                PasswordHash = _authService.HashPassword(request.Password) // Hash the password
            };
            // Save to database
            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();
            return Ok(new { username = newUser.Username });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Check database for user
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            // Verify password and user existence
            if (user == null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }
            // Generate JWT token
            var token = _authService.GenerateToken(user.Id, user.Username!);
            // Return token
            return Ok(new { access_token = token, token_type = "Bearer" });
        }
    }
}
