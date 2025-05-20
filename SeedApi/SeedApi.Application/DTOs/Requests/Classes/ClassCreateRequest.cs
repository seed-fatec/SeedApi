using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SeedApi.Application.DTOs.Requests.Classes;

public class ClassCreateRequest : IValidatableObject
{
  [Required]
  [MaxLength(255)]
  public string Name { get; set; } = null!;

  [MaxLength(2000)]
  public string? Description { get; set; }

  [Required]
  [JsonPropertyName("start_timestamp")]
  public DateTime StartTimestamp { get; set; }

  [Required]
  [JsonPropertyName("duration_minutes")]
  public int DurationMinutes { get; set; }

  public bool Free { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (StartTimestamp == default)
    {
      yield return new ValidationResult("A data de início da aula é obrigatória.", [nameof(StartTimestamp)]);
    }
    if (DurationMinutes <= 0)
    {
      yield return new ValidationResult("A duração da aula deve ser maior que zero.", [nameof(DurationMinutes)]);
    }
  }
}
