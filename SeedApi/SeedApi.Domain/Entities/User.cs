using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeedApi.Domain.Entities;

public class User
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [Required]
  [MaxLength(255)]
  public string Name { get; set; } = null!;

  [Required]
  [MaxLength(255)]
  public string Email { get; set; } = null!;

  [MaxLength(500)]
  public string? Biography { get; set; } = null!;

  [DataType(DataType.Date)]
  public DateOnly? BirthDate { get; set; } = null!;

  public string? AvatarURL { get; set; } = null!;

  [Required]
  public string PasswordHash { get; set; } = null!;

  public RefreshToken? RefreshToken { get; set; }

  [Required]
  public UserRole Role { get; set; }

  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; } = null;
  public DateTime? DeletedAt { get; set; }

  public ICollection<Course> EnrolledCourses = [];
  public ICollection<Course> TaughtCourses = [];
}

public enum UserRole
{
  Student = 1,
  Teacher
}