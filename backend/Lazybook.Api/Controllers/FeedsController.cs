using System.Security.Claims;
using Lazybook.Api.Data;
using Lazybook.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lazybook.Api.Controllers
{
    [Authorize]
    [Route("api/feeds")]
    [ApiController]
    public class FeedsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public FeedsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("home")]
        public async Task<IActionResult> GetHome()
        {
            // Check if user exists
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            // Build and return feed
            // Get all posts from users that the current user follows
            var posts = await _dbContext.Posts
                .Where(p =>
                    // For each post, check: "Is this post's UserId in the current user's following list?"
                    _dbContext.UserFollows
                        .Where(uf => uf.FollowerId == userId)   // Rows where I'm the follower
                        .Select(uf => uf.FollowingId)           // Get the user IDs I'm following
                        .Contains(p.UserId)                     // Does this post's UserId match any of them?
                )
                .OrderByDescending(p => p.CreatedAt)            // Sort by newest first
                .Select(p => new PostResponse                   // Project to PostResponse DTO
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    Text = p.Text,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
            return Ok(posts);
        }
    }
}
