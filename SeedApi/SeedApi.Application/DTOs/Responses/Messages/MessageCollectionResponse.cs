using SeedApi.Domain.Entities;

namespace SeedApi.Application.DTOs.Responses.Messages
{
  public class MessageCollectionResponse
  {
    public List<Message> Messages { get; set; } = [];
  }
}
