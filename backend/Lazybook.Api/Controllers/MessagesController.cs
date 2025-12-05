using System.Security.Claims;
using Lazybook.Api.Data;
using Lazybook.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lazybook.Api.Controllers
{
    [Authorize]
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public MessagesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetConversation([FromRoute] string username)
        {
            // Check if user exists
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (currentUser == null)
            {
                return NotFound("User not found");
            }
            // Check if other user exists
            var otherUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (otherUser == null)
            {
                return NotFound("User not found");
            }
            // Check if not the same user
            if (currentUser.Id == otherUser.Id)
            {
                return BadRequest("Cannot get conversation with yourself");
            }
            // Build and return conversation
            var messages = await _dbContext.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
                .Where(m =>
                    (m.SenderId == currentUser.Id && m.RecipientId == otherUser.Id) ||
                    (m.SenderId == otherUser.Id && m.RecipientId == currentUser.Id)
                )
                .OrderBy(m => m.CreatedAt) // Oldest first
                .Select(m => new MessageResponse
                {
                    Id = m.Id,
                    SenderUsername = m.Sender.Username,
                    SenderProfilePictureUrl = m.Sender.PictureUrl,
                    RecipientUsername = m.Recipient.Username,
                    RecipientProfilePictureUrl = m.Recipient.PictureUrl,
                    Text = m.Text,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();
            return Ok(messages);
        }
    }
}
