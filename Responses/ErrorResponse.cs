namespace SeedApi.Responses;

public record ErrorResponse
{
  public string Message { get; init; } = null!;
  public Link[] Links { get; init; } = null!;
}
