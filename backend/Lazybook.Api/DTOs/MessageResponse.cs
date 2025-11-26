using System;

namespace Lazybook.Api.DTOs;

public class MessageResponse
{
    public int Id { get; init; }
    public required string SenderUsername { get; init; }
    public required string RecipientUsername { get; init; }
    public required string Text { get; init; }
    public DateTime CreatedAt { get; init; }
}
