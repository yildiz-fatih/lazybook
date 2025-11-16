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
    [Route("api/profiles")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public ProfilesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string username)
        {
            // Handle empty search
            if (string.IsNullOrWhiteSpace(username))
            {
                return Ok(new List<ProfileSummaryResponse>());
            }
            // Search users by username starting with query
            var users = await _dbContext.Users
                .Where(u => u.Username.ToLower().StartsWith(username.ToLower()))
                .Take(10)
                .Select(u => new ProfileSummaryResponse { Id = u.Id, Username = u.Username })
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetByUsername([FromRoute] string username)
        {
            // Check if user exists
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return NotFound("User not found");
            }
            // Get stats
            var followerCount = await _dbContext.UserFollows.CountAsync(uf => uf.FollowingId == user.Id);
            var followingCount = await _dbContext.UserFollows.CountAsync(uf => uf.FollowerId == user.Id);
            // Determine relationship flags
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isSelf = currentUserId == user.Id;
            var iFollow = await _dbContext.UserFollows.AnyAsync(uf => uf.FollowerId == currentUserId && uf.FollowingId == user.Id);
            var followsMe = await _dbContext.UserFollows.AnyAsync(uf => uf.FollowerId == user.Id && uf.FollowingId == currentUserId);
            // Build and return user
            var userResponse = new ProfileDetailsResponse
            {
                Id = user.Id,
                Username = user.Username,
                Status = user.Status,
                FollowerCount = followerCount,
                FollowingCount = followingCount,
                IsSelf = isSelf,
                IFollow = iFollow,
                FollowsMe = followsMe,
            };
            return Ok(userResponse);
        }

        [HttpPost("{username}/follow")]
        public async Task<IActionResult> Follow([FromRoute] string username)
        {
            // Check if target user exists
            var targetUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (targetUser == null)
            {
                return NotFound("User not found");
            }
            // Check if the user is trying to follow themselves
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (currentUserId == targetUser.Id)
            {
                return BadRequest("Cannot follow yourself");
            }
            // Check for existing follow relationship
            var existingFollow = await _dbContext.UserFollows.FirstOrDefaultAsync(u => u.FollowerId == currentUserId && u.FollowingId == targetUser.Id);
            if (existingFollow != null)
            {
                return BadRequest("Already following");
            }
            // Create new follow relationship and return
            var newFollow = new UserFollow
            {
                FollowerId = currentUserId,
                FollowingId = targetUser.Id,
            };
            _dbContext.UserFollows.Add(newFollow);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{username}/follow")]
        public async Task<IActionResult> Unfollow([FromRoute] string username)
        {
            // Check if target user exists
            var targetUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (targetUser == null)
            {
                return NotFound("User not found");
            }
            // Check if the user is trying to unfollow themselves
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (currentUserId == targetUser.Id)
            {
                return BadRequest("Cannot unfollow yourself");
            }
            // Check for existing follow relationship
            var existingFollow = await _dbContext.UserFollows.FirstOrDefaultAsync(u => u.FollowerId == currentUserId && u.FollowingId == targetUser.Id);
            if (existingFollow == null)
            {
                return BadRequest("Cannot unfollow a user you are not following");
            }
            // Remove follow relationship and return
            _dbContext.UserFollows.Remove(existingFollow);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{username}/followers")]
        public async Task<IActionResult> GetFollowers([FromRoute] string username)
        {
            // Check if user exists
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return NotFound("User not found");
            }
            // Get and return followers
            var followers = await _dbContext.UserFollows.Where(uf => uf.FollowingId == user.Id).Select(uf => new ProfileSummaryResponse { Id = uf.Follower.Id, Username = uf.Follower.Username }).ToListAsync();
            return Ok(followers);
        }

        [HttpGet("{username}/following")]
        public async Task<IActionResult> GetFollowing([FromRoute] string username)
        {
            // Check if user exists
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return NotFound("User not found");
            }
            // Get and return following
            var following = await _dbContext.UserFollows.Where(uf => uf.FollowerId == user.Id).Select(uf => new ProfileSummaryResponse { Id = uf.Following.Id, Username = uf.Following.Username }).ToListAsync();
            return Ok(following);
        }
    }
}
