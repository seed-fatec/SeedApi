using MongoDB.Driver;
using SeedApi.Domain.Entities;

namespace SeedApi.Application.Interfaces;

public interface IMongoContext
{
  IMongoCollection<Message> Messages { get; }
}
