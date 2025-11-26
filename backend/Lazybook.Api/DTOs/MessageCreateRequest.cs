using System;
using System.ComponentModel.DataAnnotations;

namespace Lazybook.Api.DTOs;

public class MessageCreateRequest
{
    [Required]
    public required string RecipientUsername { get; init; }

    [Required]
    [MaxLength(1000)]
    public required string Text { get; init; }
}
