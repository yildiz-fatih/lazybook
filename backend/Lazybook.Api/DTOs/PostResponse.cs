using System;

namespace Lazybook.Api.DTOs;

public class PostResponse
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public required string Username { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public required string Text { get; init; }
    public DateTime CreatedAt { get; init; }
}
