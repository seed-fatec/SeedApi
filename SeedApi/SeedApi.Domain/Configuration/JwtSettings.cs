namespace SeedApi.Domain.Configuration;

public class JwtSettings
{
  public string Secret { get; set; } = null!;
  public string Issuer { get; set; } = null!;
  public string Audience { get; set; } = null!;
}