using System.Text.Json.Serialization;

namespace SeedApi.Responses.Auth;

public record LoginResponse
{
  [JsonPropertyName("access_token")]
  public string AccessToken { get; init; } = null!;

  [JsonPropertyName("refresh_token")]
  public string RefreshToken { get; init; } = null!;
}