using System.Text.Json.Serialization;

namespace SeedApi.Application.DTOs.Responses.Auth;

public record RefreshResponse
{
  [JsonPropertyName("access_token")]
  public string AccessToken { get; init; } = null!;
}