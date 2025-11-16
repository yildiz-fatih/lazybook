using System;

namespace Lazybook.Api.DTOs;

public class AccountUpdateResponse
{
    public int Id { get; init; }
    public string? Username { get; init; }
    public required string Status { get; init; }
}