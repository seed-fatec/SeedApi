using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeedApi.Models.Entities;

public class Admin
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [Required]
  [MaxLength(255)]
  public string Email { get; set; } = null!;

  [Required]
  public string PasswordHash { get; set; } = null!;

  public AdminRefreshToken? RefreshToken { get; set; }

  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; } = null;
  public DateTime? DeletedAt { get; set; }
}
