namespace SeedApi.Responses;

public record ErrorResponse
{
  public string Message { get; init; } = null!;
}
