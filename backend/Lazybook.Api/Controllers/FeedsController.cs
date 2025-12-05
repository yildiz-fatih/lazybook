using System.Security.Claims;
using System.Text.Json;
using Lazybook.Api.Data;
using Lazybook.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Lazybook.Api.Controllers
{
    [Authorize]
    [Route("api/feeds")]
    [ApiController]
    public class FeedsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IDistributedCache _cache;
        public FeedsController(AppDbContext dbContext, IDistributedCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
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
                    ProfilePictureUrl = p.User.PictureUrl,
                    Text = p.Text,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
            return Ok(posts);
        }

        [HttpGet("explore")]
        public async Task<IActionResult> GetExplore()
        {
            // Define a unique key for this data in Redis
            string cacheKey = "feeds:explore";

            // Try to get data from Redis cache ("Hit")
            string? cachedJson = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedJson))
            {
                // It's a hit! Convert string back to List<PostResponse>
                var cachedPosts = JsonSerializer.Deserialize<List<PostResponse>>(cachedJson);
                // Return immediately
                return Ok(cachedPosts);
            }

            // Data not in cache ("Miss") -> Query Database
            // Build and return explore feed
            var posts = await _dbContext.Posts
                .OrderByDescending(p => p.CreatedAt)    // Sort by newest first
                .Take(50)                               // Limit to 50 posts
                .Select(p => new PostResponse           // Project to PostResponse DTO
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    ProfilePictureUrl = p.User.PictureUrl,
                    Text = p.Text,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            // Save to Redis cache for next time
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            };
            string postsJson = JsonSerializer.Serialize(posts);
            await _cache.SetStringAsync(cacheKey, postsJson, cacheOptions);

            return Ok(posts);
        }
    }
}
