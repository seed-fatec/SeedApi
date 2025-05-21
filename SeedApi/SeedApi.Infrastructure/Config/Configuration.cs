using Microsoft.Extensions.Configuration;
using SeedApi.Domain.Configuration;

namespace SeedApi.Infrastructure.Config;

public class Configuration
{
  public DatabaseSettings DatabaseSettings { get; }
  public JwtSettings JwtSettings { get; }
  public AdminSettings AdminSettings { get; }

  public Configuration(IConfiguration configuration)
  {
    DatabaseSettings = configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>()
        ?? throw new Exception("Failed to load DatabaseSettings from environment.");
    JwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
        ?? throw new Exception("Failed to load JwtSettings from environment.");
    AdminSettings = configuration.GetSection("Admin").Get<AdminSettings>()
        ?? throw new Exception("Failed to load Admin Credentials from environment.");

    ValidateSettings(DatabaseSettings);
    ValidateSettings(JwtSettings);
    ValidateSettings(AdminSettings);
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
