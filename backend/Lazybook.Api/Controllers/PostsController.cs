using System.Security.Claims;
using Lazybook.Api.Data;
using Lazybook.Api.DTOs;
using Lazybook.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lazybook.Api.Controllers
{
    [Authorize]
    [Route("api/posts")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public PostsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById([FromRoute] int id)
        {
            // Check if post exists
            var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound("Post not found");
            }
            // Build and return post
            var postResponse = new PostResponse
            {
                Id = post.Id,
                UserId = post.UserId,
                Username = post.User.Username,
                Text = post.Text,
                CreatedAt = post.CreatedAt
            };
            return Ok(postResponse);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] PostCreateRequest request)
        {
            // Check if user exists
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            // Create new post
            var post = new Post
            {
                UserId = user.Id,
                Text = request.Text
            };
            _dbContext.Posts.Add(post);
            await _dbContext.SaveChangesAsync();
            // Build and return post
            var postResponse = new PostResponse
            {
                Id = post.Id,
                UserId = post.UserId,
                Username = user.Username,
                Text = post.Text,
                CreatedAt = post.CreatedAt
            };
            return CreatedAtAction(nameof(GetPostById), new { id = postResponse.Id }, postResponse);
        }

        [HttpGet]
        public async Task<IActionResult> GetFeed()
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
