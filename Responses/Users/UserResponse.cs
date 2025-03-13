namespace SeedApi.Responses.Users
{
  public record UserResponse
  {
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;
    public Link[] Links { get; init; } = null!;
  }
}