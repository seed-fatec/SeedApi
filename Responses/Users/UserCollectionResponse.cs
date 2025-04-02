namespace SeedApi.Responses.Users;

public record UserCollectionResponse
{
  public List<PublicUserResponse> Users { get; init; } = [];
}