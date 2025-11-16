using System.Security.Claims;
using Lazybook.Api.Data;
using Lazybook.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lazybook.Api.Controllers
{
    [Authorize]
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public AccountController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetMe()
        {
            // Check if user exists
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            // Get stats
            var followerCount = await _dbContext.UserFollows.CountAsync(uf => uf.FollowingId == user.Id);
            var followingCount = await _dbContext.UserFollows.CountAsync(uf => uf.FollowerId == user.Id);
            // Build and return user
            var userResponse = new AccountDetailsResponse
            {
                Id = user.Id,
                Username = user.Username,
                Status = user.Status,
                FollowerCount = followerCount,
                FollowingCount = followingCount
            };
            return Ok(userResponse);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMe([FromBody] AccountUpdateRequest request)
        {
            // Check if user exists
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            // Update user
            user.Status = request.Status;
            await _dbContext.SaveChangesAsync();
            // Return updated user
            var userResponse = new AccountUpdateResponse
            {
                Id = user.Id,
                Username = user.Username,
                Status = user.Status,
            };
            return Ok(userResponse);
        }
    }
}
