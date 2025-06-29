using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SeedApi.Application.Interfaces;
using SeedApi.Domain.Entities;

namespace SeedApi.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IPersistenceContext
{
  public DbSet<Admin> Admins { get; set; }
  public DbSet<User> Users { get; set; }
  public DbSet<Course> Courses { get; set; }
  public DbSet<RefreshToken> RefreshTokens { get; set; }
  public DbSet<AdminRefreshToken> AdminRefreshTokens { get; set; }
  public DbSet<Class> Classes { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Configuração de filtros globais para soft delete
    modelBuilder.Entity<Admin>().HasQueryFilter(a => a.DeletedAt == null);
    modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
    modelBuilder.Entity<RefreshToken>().HasQueryFilter(rt => rt.User != null && rt.User.DeletedAt == null);
    modelBuilder.Entity<AdminRefreshToken>().HasQueryFilter(rt => rt.Admin != null && rt.Admin.DeletedAt == null);
    modelBuilder.Entity<Course>().HasQueryFilter(c => c.DeletedAt == null);
    modelBuilder.Entity<Class>().HasQueryFilter(c => c.Course != null && c.DeletedAt == null);

    // Configuração de relacionamentos
    modelBuilder.Entity<Course>()
      .HasMany(c => c.Students)
      .WithMany(u => u.EnrolledCourses)
      .UsingEntity(j => j.ToTable("CourseStudents"));

    modelBuilder.Entity<Course>()
      .HasMany(c => c.Teachers)
      .WithMany(u => u.TaughtCourses)
      .UsingEntity(j => j.ToTable("CourseTeachers"));

    modelBuilder.Entity<Class>()
      .HasOne(c => c.Course)
      .WithMany(c => c.Classes)
      .HasForeignKey(c => c.CourseId)
      .OnDelete(DeleteBehavior.Restrict);
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
      // Só aplica soft delete em coleções ou propriedades de navegação que são filhos
      // Ignora navegação para o pai (ex: Course de Class)
      if (navigation.Metadata is not { IsCollection: false, TargetEntityType.ClrType.Name: "Course" })
      {
        if (navigation.CurrentValue is IEnumerable<object> relatedEntities)
        {
          foreach (var relatedEntity in relatedEntities)
          {
            ApplySoftDeleteToEntity(relatedEntity);
          }
        }
        else if (navigation.CurrentValue is object relatedEntity)
        {
          ApplySoftDeleteToEntity(relatedEntity);
        }
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