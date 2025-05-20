using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeedApi.Domain.Entities;

public class Class
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [Required]
  [MaxLength(255)]
  public string Name { get; set; } = null!;

  [MaxLength(2000)]
  public string? Description { get; set; }

  [Required]
  public DateTime StartTimestamp { get; set; }

  [Required]
  public int DurationMinutes { get; set; }

  [Required]
  public bool IsFree { get; set; }

  [Required]
  public int CourseId { get; set; }

  [ForeignKey(nameof(CourseId))]
  public Course Course { get; set; } = null!;

  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; } = null;
  public DateTime? DeletedAt { get; set; }
}