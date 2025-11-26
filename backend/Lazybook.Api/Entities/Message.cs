using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lazybook.Api.Entities;

[Table("messages")]
public class Message
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int SenderId { get; set; }

    [Required]
    public int RecipientId { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string Text { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(SenderId))]
    public User Sender { get; set; } = null!;

    [ForeignKey(nameof(RecipientId))]
    public User Recipient { get; set; } = null!;
}
