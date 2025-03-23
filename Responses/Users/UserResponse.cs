using SeedApi.Models;

namespace SeedApi.Responses.Users
{
  public record UserResponse
  {
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;
    public UserRole Role { get; init; }
    public DateOnly? BirthDate { get; init; }
    public Link[] Links { get; init; } = null!;
  }
}