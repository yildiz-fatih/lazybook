using Lazybook.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lazybook.Api.Data;

public class AppDbContext : DbContext
{
    // Constructor accepts database configuration options
    // These options are configured in Program.cs using AddDbContext
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) // Pass the options to the parent DbContext class constructor
    {
        // Custom initialization logic can go here if needed
    }

    // Each DbSet corresponds to a table in the database
    public DbSet<User> Users { get; set; } = null!; // Will be initialized by EF Core, use 'null!' to suppress nullable warning
    public DbSet<UserFollow> UserFollows { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
}
