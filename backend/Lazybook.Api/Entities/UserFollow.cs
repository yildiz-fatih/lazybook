using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Lazybook.Api.Entities;

[Table("user_follows")]
[PrimaryKey(nameof(FollowerId), nameof(FollowingId))]
public class UserFollow
{
    public int FollowerId { get; set; }
    public int FollowingId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(FollowerId))]
    public User Follower { get; set; } = null!;

    [ForeignKey(nameof(FollowingId))]
    public User Following { get; set; } = null!;
}
