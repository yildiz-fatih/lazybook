using System.Security.Claims;
using Lazybook.Api.Data;
using Lazybook.Api.DTOs;
using Lazybook.Api.Services;
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
        private readonly FileStorageService _fileStorageService;

        public AccountController(AppDbContext dbContext, FileStorageService fileStorageService)
        {
            _dbContext = dbContext;
            _fileStorageService = fileStorageService;
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
                ProfilePictureUrl = user.PictureUrl,
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

        [HttpPost("picture")]
        public async Task<IActionResult> UploadPicture([FromForm] AccountPictureRequest request)
        {
            // Check if user exists
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            // Save image to disk
            var imageUrl = await _fileStorageService.SaveFileAsync(request.Image);
            // Update database
            user.PictureUrl = imageUrl;
            await _dbContext.SaveChangesAsync();
            // Return image URL
            return Ok(imageUrl);
        }
    }
}
