using System.Text.Json.Serialization;

namespace SeedApi.Application.DTOs.Responses.Courses;

public record CourseResponse
{
  public int Id { get; init; }

  public string Name { get; init; } = null!;

  public string? Description { get; init; } = null!;

  [JsonPropertyName("avatar_url")]
  public string? AvatarURL { get; set; } = null!;

  public uint Price { get; init; }

  [JsonPropertyName("max_capacity")]
  public uint MaxCapacity { get; init; }

  [JsonPropertyName("start_date")]
  public DateOnly StartDate { get; init; }

  [JsonPropertyName("end_date")]
  public DateOnly EndDate { get; init; }

  [JsonPropertyName("remaining_vacancies")]
  public int RemainingVacancies { get; init; }

  [JsonPropertyName("created_at")]
  public DateTime CreatedAt { get; init; }

  [JsonPropertyName("updated_at")]
  public DateTime? UpdatedAt { get; init; }
}
