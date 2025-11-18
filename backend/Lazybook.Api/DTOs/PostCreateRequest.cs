using System;

namespace Lazybook.Api.DTOs;

using System.ComponentModel.DataAnnotations;

public class PostCreateRequest
{
    [Required]
    [MaxLength(280)]
    public required string Text { get; init; }
}
