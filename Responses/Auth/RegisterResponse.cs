namespace SeedApi.Responses.Auth;

public record RegisterResponse
{
  public string Message { get; init; } = null!;
}