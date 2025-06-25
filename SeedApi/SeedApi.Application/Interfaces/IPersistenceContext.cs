using Microsoft.EntityFrameworkCore;
using SeedApi.Domain.Entities;

namespace SeedApi.Application.Interfaces;

public interface IPersistenceContext
{
  public DbSet<Admin> Admins { get; }
  public DbSet<User> Users { get; }
  public DbSet<Course> Courses { get; }
  public DbSet<Class> Classes { get; }
  public DbSet<RefreshToken> RefreshTokens { get; }
  public DbSet<AdminRefreshToken> AdminRefreshTokens { get; }

  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}