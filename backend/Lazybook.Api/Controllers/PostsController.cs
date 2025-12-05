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
                ProfilePictureUrl = post.User.PictureUrl,
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
                ProfilePictureUrl = user.PictureUrl,
                Text = post.Text,
                CreatedAt = post.CreatedAt
            };
            return CreatedAtAction(nameof(GetPostById), new { id = postResponse.Id }, postResponse);
        }
    }
}
