using SeedApi.Domain.Entities;
using System.Text.Json.Serialization;

namespace SeedApi.Application.DTOs.Responses.Users;

public record UserResponse
{
  public int Id { get; init; }
  public string Name { get; init; } = null!;
  public string? Biography { get; init; } = null!;
  public string Email { get; init; } = null!;
  public UserRole Role { get; init; }

  [JsonPropertyName("avatar_url")]
  public string? AvatarURL { get; init; }

  [JsonPropertyName("birth_date")]
  public DateOnly? BirthDate { get; init; }

  [JsonPropertyName("created_at")]
  public DateTime CreatedAt { get; init; }

  [JsonPropertyName("updated_at")]
  public DateTime? UpdatedAt { get; init; }
}