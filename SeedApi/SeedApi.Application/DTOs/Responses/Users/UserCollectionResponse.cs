namespace SeedApi.Application.DTOs.Responses.Users;

public record UserCollectionResponse
{
  public List<PublicUserResponse> Users { get; init; } = [];
}