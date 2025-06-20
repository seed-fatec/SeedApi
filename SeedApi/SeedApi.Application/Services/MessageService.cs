using MongoDB.Driver;
using SeedApi.Domain.Entities;
using SeedApi.Application.Interfaces;

namespace SeedApi.Application.Services
{
  public class MessageService(IMongoContext mongoMessageContext)
  {
    private readonly IMongoCollection<Message> _messages = mongoMessageContext.Messages;

    public async Task AddMessageAsync(Message message)
    {
      await _messages.InsertOneAsync(message);
    }

    public async Task<List<Message>> GetMessagesBetweenUsersAsync(int userId1, int userId2)
    {
      var filter = Builders<Message>.Filter.Or(
          Builders<Message>.Filter.And(
              Builders<Message>.Filter.Eq(m => m.SenderId, userId1),
              Builders<Message>.Filter.Eq(m => m.RecipientId, userId2)
          ),
          Builders<Message>.Filter.And(
              Builders<Message>.Filter.Eq(m => m.SenderId, userId2),
              Builders<Message>.Filter.Eq(m => m.RecipientId, userId1)
          )
      );
      return await _messages.Find(filter).SortBy(m => m.Timestamp).ToListAsync();
    }
  }
}
