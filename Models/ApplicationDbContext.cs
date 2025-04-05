using Microsoft.EntityFrameworkCore;
using SeedApi.Models.Entities;

namespace SeedApi.Models;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
  public DbSet<User> Users { get; set; }
  public DbSet<Course> Courses { get; set; }
  public DbSet<RefreshToken> RefreshTokens { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Course>()
      .HasMany(c => c.Students)
      .WithMany(u => u.EnrolledCourses)
      .UsingEntity(j => j.ToTable("CourseStudents"));

    modelBuilder.Entity<Course>()
      .HasMany(c => c.Teachers)
      .WithMany(u => u.TaughtCourses)
      .UsingEntity(j => j.ToTable("CourseTeachers"));
  }

  public override int SaveChanges()
  {
    foreach (var entry in ChangeTracker.Entries())
    {
      if (entry.Entity is not null && entry.State is EntityState.Added or EntityState.Modified)
      {
        var now = DateTime.UtcNow;
        if (entry.State == EntityState.Added && entry.Properties.Any(p => p.Metadata.Name == "CreatedAt"))
          entry.Property("CreatedAt").CurrentValue = now;

        if (entry.Properties.Any(p => p.Metadata.Name == "UpdatedAt"))
          entry.Property("UpdatedAt").CurrentValue = now;
      }
    }
    return base.SaveChanges();
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    foreach (var entry in ChangeTracker.Entries())
    {
      if (entry.Entity is not null && entry.State is EntityState.Added or EntityState.Modified)
      {
        var now = DateTime.UtcNow;
        if (entry.State == EntityState.Added && entry.Properties.Any(p => p.Metadata.Name == "CreatedAt"))
          entry.Property("CreatedAt").CurrentValue = now;

        if (entry.Properties.Any(p => p.Metadata.Name == "UpdatedAt"))
          entry.Property("UpdatedAt").CurrentValue = now;
      }
    }
    return await base.SaveChangesAsync(cancellationToken);
  }
}