using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeedApi.Models.Entities;

public class Course
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [Required]
  [MaxLength(255)]
  public string Name { get; set; } = null!;

  [Required]
  public uint Price { get; set; }

  [Required]
  public uint MaxCapacity { get; set; }

  [Required]
  public DateOnly StartDate { get; set; }

  [Required]
  public DateOnly EndDate { get; set; }

  [MaxLength(2000)]
  public string? Description { get; set; }

  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; } = null;
  public DateTime? DeletedAt { get; set; }

  public ICollection<User> Students { get; set; } = [];
  public ICollection<User> Teachers { get; set; } = [];
}