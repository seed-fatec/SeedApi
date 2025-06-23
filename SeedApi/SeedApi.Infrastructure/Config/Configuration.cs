using Microsoft.Extensions.Configuration;
using SeedApi.Domain.Configuration;

namespace SeedApi.Infrastructure.Config;

public class Configuration
{
  public MySqlSettings MySqlSettings { get; }
  public MongoSettings MongoSettings { get; }
  public JwtSettings JwtSettings { get; }
  public AdminSettings AdminSettings { get; }
  public AzureSettings AzureSettings { get; }

  public Configuration(IConfiguration configuration)
  {
    MySqlSettings = configuration.GetSection("MySqlSettings").Get<MySqlSettings>()
        ?? throw new Exception("Failed to load MySqlSettings from environment.");

    MongoSettings = configuration.GetSection("MongoSettings").Get<MongoSettings>()
        ?? throw new Exception("Failed to load MongoSettings from environment.");

    JwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
        ?? throw new Exception("Failed to load JwtSettings from environment.");

    AdminSettings = configuration.GetSection("Admin").Get<AdminSettings>()
        ?? throw new Exception("Failed to load Admin Credentials from environment.");

    AzureSettings = configuration.GetSection("AzureSettings").Get<AzureSettings>()
        ?? throw new Exception("Failed to load AzureSettings from environment.");

    ValidateSettings(MySqlSettings);
    ValidateSettings(MongoSettings);
    ValidateSettings(JwtSettings);
    ValidateSettings(AdminSettings);
    ValidateSettings(AzureSettings);
  }

  private static void ValidateSettings(object settings)
  {
    var missing = new List<string>();
    foreach (var prop in settings.GetType().GetProperties())
    {
      var value = prop.GetValue(settings);
      if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
        missing.Add(prop.Name);
    }
    if (missing.Count > 0)
      throw new Exception($"Missing required configuration values: {string.Join(", ", missing)}");
  }
}
