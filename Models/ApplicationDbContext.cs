using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

    // Configuração de filtros globais para soft delete
    modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
    modelBuilder.Entity<RefreshToken>().HasQueryFilter(rt => rt.User != null && rt.User.DeletedAt == null);
    modelBuilder.Entity<Course>().HasQueryFilter(c => c.DeletedAt == null);

    // Configuração de relacionamentos
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
    ApplyTimestamps();
    ApplySoftDeleteCascade();
    return base.SaveChanges();
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    ApplyTimestamps();
    ApplySoftDeleteCascade();
    return await base.SaveChangesAsync(cancellationToken);
  }

  private void ApplyTimestamps()
  {
    foreach (var entry in ChangeTracker.Entries())
    {
      if (entry.Entity is not null && entry.State is EntityState.Added or EntityState.Modified)
      {
        var now = DateTime.UtcNow;

        // Define CreatedAt para entidades novas
        if (entry.State == EntityState.Added && entry.Properties.Any(p => p.Metadata.Name == "CreatedAt"))
        {
          entry.Property("CreatedAt").CurrentValue = now;
        }

        // Atualiza UpdatedAt para entidades modificadas
        if (entry.Properties.Any(p => p.Metadata.Name == "UpdatedAt"))
        {
          entry.Property("UpdatedAt").CurrentValue = now;
        }
      }
    }
  }

  private void ApplySoftDeleteCascade()
  {
    foreach (var entry in ChangeTracker.Entries())
    {
      if (entry.Entity is not null && entry.State == EntityState.Deleted)
      {
        // Verifica se a entidade suporta soft delete
        if (entry.Properties.Any(p => p.Metadata.Name == "DeletedAt"))
        {
          // Aplica soft delete na entidade principal
          entry.State = EntityState.Modified;
          entry.Property("DeletedAt").CurrentValue = DateTime.UtcNow;

          // Aplica soft delete em entidades relacionadas
          ApplySoftDeleteToRelatedEntities(entry);
        }
      }
    }
  }

  private void ApplySoftDeleteToRelatedEntities(EntityEntry entry)
  {
    foreach (var navigation in entry.Navigations)
    {
      if (navigation.CurrentValue is IEnumerable<object> relatedEntities)
      {
        // Para coleções de entidades relacionadas
        foreach (var relatedEntity in relatedEntities)
        {
          ApplySoftDeleteToEntity(relatedEntity);
        }
      }
      else if (navigation.CurrentValue is object relatedEntity)
      {
        // Para entidades relacionadas individuais
        ApplySoftDeleteToEntity(relatedEntity);
      }
    }
  }

  private void ApplySoftDeleteToEntity(object relatedEntity)
  {
    var relatedEntry = Entry(relatedEntity);

    // Verifica se a entidade relacionada suporta soft delete
    if (relatedEntry.Properties.Any(p => p.Metadata.Name == "DeletedAt"))
    {
      if (relatedEntry.State != EntityState.Deleted)
      {
        relatedEntry.State = EntityState.Modified;
        relatedEntry.Property("DeletedAt").CurrentValue = DateTime.UtcNow;
      }
    }
  }
}