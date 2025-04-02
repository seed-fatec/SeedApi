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
}