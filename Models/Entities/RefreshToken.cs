using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeedApi.Models.Entities;

public class RefreshToken
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [Required]
  public string Token { get; set; } = null!;

  [Required]
  public DateTime ExpiryTime { get; set; }

  [Required]
  public int UserId { get; set; }

  [ForeignKey(nameof(UserId))]
  public User User { get; set; } = null!;
}