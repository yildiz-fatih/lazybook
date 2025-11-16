using System;

namespace Lazybook.Api.DTOs;

public class AccountDetailsResponse
{
    public int Id { get; init; }
    public string? Username { get; init; }
    public required string Status { get; init; }

    // Calculated fields
    public int FollowerCount { get; init; }
    public int FollowingCount { get; init; }
}