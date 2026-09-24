using Microsoft.EntityFrameworkCore;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<project.Models.NW.User> User { get; set; } = default!;
}
