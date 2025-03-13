namespace SeedApi.Responses
{
  public record Link
  {
    public required string Rel { get; init; }
    public string? Href { get; init; }
  }
}