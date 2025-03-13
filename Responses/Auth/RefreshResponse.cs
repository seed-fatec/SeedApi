namespace SeedApi.Responses.Auth
{
  public record RefreshResponse
  {
    public string AccessToken { get; init; } = null!;
    public Link[] Links { get; init; } = null!;
  }
}