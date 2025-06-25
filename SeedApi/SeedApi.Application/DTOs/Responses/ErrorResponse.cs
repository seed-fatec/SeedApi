namespace SeedApi.Application.DTOs.Responses;

public record ErrorResponse
{
  public string Message { get; init; } = null!;
}
