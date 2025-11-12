using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lazybook.Api.Entities;

public enum AuthProviderEnum
{
    Local,
    Google,
    GitHub
}

[Table("users")]
[Index(nameof(AuthProvider), nameof(AuthProviderUserId), IsUnique = true)]
public class User
{
    [Key]
    public int Id { get; set; }

    [MaxLength(64)]
    public string? Username { get; set; }

    [MaxLength(128)]
    [Column("password_hash")]
    public required string PasswordHash { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;

    [Required]
    public AuthProviderEnum AuthProvider { get; set; } = AuthProviderEnum.Local;

    public string? AuthProviderUserId { get; set; }
}

