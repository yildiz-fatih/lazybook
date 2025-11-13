using System;
using System.ComponentModel.DataAnnotations;

namespace Lazybook.Api.DTOs;

public class RegisterRequest
{
    [Required]
    public required string Username { get; init; }

    [Required]
    [StringLength(64, MinimumLength = 12)]
    public required string Password { get; init; }
}
