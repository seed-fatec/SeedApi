namespace SeedApi.Responses.Auth
{
  public record LoginResponse
  {
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
    public Link[] Links { get; init; } = null!;
  }
}