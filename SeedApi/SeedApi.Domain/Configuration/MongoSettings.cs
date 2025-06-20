namespace SeedApi.Domain.Configuration;

public class MongoSettings
{
  public string DatabaseName { get; set; } = null!;
  public string ConnectionString { get; set; } = null!;
}
