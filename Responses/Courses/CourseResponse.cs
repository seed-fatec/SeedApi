using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SeedApi.Responses.Courses;

public record CourseResponse
{
  public int Id { get; init; }

  public string Name { get; init; } = null!;

  public string? Description { get; init; } = null!;

  public uint Price { get; init; }

  [JsonPropertyName("max_capacity")]
  public uint MaxCapacity { get; init; }

  [JsonPropertyName("start_date")]
  public DateOnly StartDate { get; init; }

  [JsonPropertyName("end_date")]
  public DateOnly EndDate { get; init; }
}
