namespace SeedApi.Responses.Auth
{
  public record RegisterResponse
  {
    public string Message { get; init; } = null!;
    public Link[] Links { get; init; } = null!;
  }
}