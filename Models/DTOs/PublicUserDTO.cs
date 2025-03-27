using System.Text.Json.Serialization;

namespace SeedApi.Models.DTOs;

public record PublicUserDTO
{
  public int Id { get; init; }
  public string Name { get; init; } = null!;

  [JsonPropertyName("birth_date")]
  public DateOnly? BirthDate { get; init; }
}