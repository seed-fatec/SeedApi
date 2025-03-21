using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeedApi.Models
{
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

    [DataType(DataType.Date)]
    public DateOnly? BirthDate { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    public RefreshToken? RefreshToken { get; set; }

    [Required]
    public UserRole Role { get; set; }
  }

  public enum UserRole
  {
    Student = 1,
    Teacher
  }
}