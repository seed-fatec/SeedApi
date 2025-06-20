using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SeedApi.Domain.Configuration;

namespace SeedApi.Infrastructure.Persistence;

public class MongoDbContext
{
  private readonly IMongoDatabase _database;
  private readonly MongoClient _mongoClient;

  public MongoDbContext(IOptions<MongoSettings> settings)
  {
    var mongoSettings = settings.Value;
    _mongoClient = new MongoClient(mongoSettings.ConnectionString);
    _database = _mongoClient.GetDatabase(mongoSettings.DatabaseName);
  }

  public IMongoCollection<T> GetCollection<T>(string name)
  {
    return _database.GetCollection<T>(name);
  }

  public IMongoDatabase GetDatabase()
  {
    return _database;
  }
}
