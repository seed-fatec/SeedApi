using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SeedApi.Requests.Courses
{
  public record CourseCreateRequest : IValidatableObject
  {
    [Required(ErrorMessage = "O nome do curso é obrigatório.")]
    [Description("O nome do curso.")]
    public string Name { get; init; } = null!;

    [Description("A descrição do curso.")]
    public string? Description { get; init; } = null!;

    [Required(ErrorMessage = "O preço do curso é obrigatório.")]
    [Description("O preço do curso.")]
    public uint Price { get; init; }

    [Required(ErrorMessage = "A capacidade máxima do curso é obrigatória.")]
    [Description("A capacidade máxima de membros do curso.")]
    [JsonPropertyName("max_capacity")]
    public uint MaxCapacity { get; init; }

    [Required(ErrorMessage = "A data de início do curso é obrigatória.")]
    [Description("A data de início do curso.")]
    [JsonPropertyName("start_date")]
    public DateOnly StartDate { get; init; }

    [Required(ErrorMessage = "A data de término do curso é obrigatória.")]
    [Description("A data de término do curso.")]
    [JsonPropertyName("end_date")]
    public DateOnly EndDate { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (StartDate == DateOnly.MinValue && EndDate == DateOnly.MinValue)
      {
        yield return new ValidationResult(
          "As datas de início e término são obrigatórias.",
          [nameof(StartDate), nameof(EndDate)]
        );
      }

      if (StartDate < DateOnly.FromDateTime(DateTime.Now.Date))
      {
        yield return new ValidationResult(
          "A data de início não pode ser anterior à data atual.",
          [nameof(StartDate)]
        );
      }

      if (EndDate < StartDate)
      {
        yield return new ValidationResult(
          "A data de término não pode ser anterior à data de início.",
          [nameof(EndDate)]
        );
      }
    }
  }
}