namespace SeedApi.Responses.Auth
{
  public record LogoutResponse
  {
    public string Message { get; init; } = null!;
    public Link[] Links { get; init; } = null!;
  }
}