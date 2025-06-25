using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SeedApi.Application.DTOs.Requests.Classes;

public class ClassUpdateRequest : IValidatableObject
{
  [Required(ErrorMessage = "O nome da aula é obrigatório.")]
  public string Name { get; set; } = null!;

  [Required(ErrorMessage = "A descrição da aula é obrigatória.")]
  public string Description { get; set; } = null!;

  [Required(ErrorMessage = "O horário de início é obrigatório.")]
  [JsonPropertyName("start_timestamp")]
  public DateTime StartTimestamp { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (StartTimestamp == default)
    {
      yield return new ValidationResult(
        "A data de início é obrigatória.",
        [nameof(StartTimestamp)]
      );
    }
  }
}
