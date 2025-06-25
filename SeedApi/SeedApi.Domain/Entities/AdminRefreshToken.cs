using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeedApi.Domain.Entities;

public class AdminRefreshToken
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [Required]
  public string Token { get; set; } = null!;

  [Required]
  public DateTime ExpiryTime { get; set; }

  [Required]
  public int AdminId { get; set; }

  [ForeignKey(nameof(AdminId))]
  public Admin Admin { get; set; } = null!;
}