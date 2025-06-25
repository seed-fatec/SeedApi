namespace SeedApi.Domain.Configuration;

public class AzureSettings
{
  public string BlobStorageConnectionString { get; set; } = null!;
  public string BlobContainerName { get; set; } = "users";
}
