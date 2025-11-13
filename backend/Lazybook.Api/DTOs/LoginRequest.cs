using System;
using System.ComponentModel.DataAnnotations;

namespace Lazybook.Api.DTOs;

public class LoginRequest
{
    [Required]
    public required string Username { get; init; }

    [Required]
    public required string Password { get; init; }
}
