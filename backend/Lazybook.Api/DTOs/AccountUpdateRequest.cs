using System;

namespace Lazybook.Api.DTOs;

public class AccountUpdateRequest
{
    public required string Status { get; init; }
}