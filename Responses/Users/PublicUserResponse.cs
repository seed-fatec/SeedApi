using SeedApi.Models.Entities;
using System.Text.Json.Serialization;

namespace SeedApi.Responses.Users;

public record PublicUserResponse
{
  public int Id { get; init; }
  public string Name { get; init; } = null!;
  public UserRole Role { get; init; }

  [JsonPropertyName("birth_date")]
  public DateOnly? BirthDate { get; init; }
}