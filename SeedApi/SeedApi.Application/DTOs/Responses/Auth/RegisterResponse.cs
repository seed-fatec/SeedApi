namespace SeedApi.Application.DTOs.Responses.Auth;

public record RegisterResponse
{
  public string Message { get; init; } = null!;
}