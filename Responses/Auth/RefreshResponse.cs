using System.Text.Json.Serialization;

namespace SeedApi.Responses.Auth;

public record RefreshResponse
{
  [JsonPropertyName("access_token")]
  public string AccessToken { get; init; } = null!;
  public Link[] Links { get; init; } = null!;
}