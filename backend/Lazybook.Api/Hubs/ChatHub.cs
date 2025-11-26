using System;
using System.Security.Claims;
using Lazybook.Api.Data;
using Lazybook.Api.DTOs;
using Lazybook.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Lazybook.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _dbContext;

    public ChatHub(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SendMessage(MessageCreateRequest request)
    {
        // Check if sender exists
        var senderId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var sender = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == senderId);
        if (sender == null)
        {
            await Clients.Caller.SendAsync("Error", "Sender not found");
            return;
        }
        // Check if recipient exists
        var recipient = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.RecipientUsername);
        if (recipient == null)
        {
            await Clients.Caller.SendAsync("Error", "Recipient not found");
            return;
        }
        // Check if not the same user
        if (sender.Id == recipient.Id)
        {
            await Clients.Caller.SendAsync("Error", "Cannot message yourself");
            return;
        }
        // Save message to database
        var message = new Message
        {
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Text = request.Text
        };
        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();
        // Send to recipient
        var messageResponse = new MessageResponse
        {
            Id = message.Id,
            SenderUsername = sender.Username,
            RecipientUsername = recipient.Username,
            Text = message.Text,
            CreatedAt = message.CreatedAt
        };
        await Clients.User(recipient.Id.ToString()).SendAsync("ReceiveMessage", messageResponse);
        // Send confirmation to sender
        await Clients.Caller.SendAsync("ReceiveMessage", messageResponse);
    }
}
