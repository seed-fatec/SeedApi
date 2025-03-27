using SeedApi.Models.DTOs;

namespace SeedApi.Responses.Users;

public record UserCollectionResponse
{
  public List<PublicUserDTO> Users { get; init; } = [];
}