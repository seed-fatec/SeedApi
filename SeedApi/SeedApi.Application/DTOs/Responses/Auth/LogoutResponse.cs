namespace SeedApi.Application.DTOs.Responses.Auth;
public record LogoutResponse
{
  public string Message { get; init; } = null!;
}
