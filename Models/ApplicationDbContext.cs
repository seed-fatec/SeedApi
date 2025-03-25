using Microsoft.EntityFrameworkCore;
using SeedApi.Models.Entities;

namespace SeedApi.Models
{
  public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
  {
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
  }
}