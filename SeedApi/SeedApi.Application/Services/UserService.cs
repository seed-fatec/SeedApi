using Microsoft.EntityFrameworkCore;
using SeedApi.Application.DTOs.Requests.Users;
using SeedApi.Application.Interfaces;
using SeedApi.Domain.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SeedApi.Domain.Configuration;

namespace SeedApi.Application.Services;

public class UserService(IPersistenceContext context, AzureSettings azureSettings)
{
  private readonly IPersistenceContext _context = context;
  private readonly AzureSettings _azureSettings = azureSettings;

  public async Task<List<User>> GetAllUsersAsync()
  {
    return await _context.Users
      .ToListAsync();
  }

  public async Task<User?> GetUserByIdAsync(int userId)
  {
    return await _context.Users
      .Where(u => u.Id == userId)
      .FirstOrDefaultAsync();
  }

  public async Task<User?> GetAuthenticatedUserAsync(ClaimsPrincipal user)
  {
    var userIdClaim = user.FindFirstValue("UserId");
    if (int.TryParse(userIdClaim, out var userId))
    {
      return await GetUserByIdAsync(userId);
    }
    return null;
  }

  public async Task<bool> UpdateUserAsync(int userId, UserUpdateRequest newUser)
  {
    var user = await _context.Users.FindAsync(userId);

    if (user == null)
      return false;

    user.Name = newUser.Name;
    user.Email = newUser.Email;
    user.Biography = newUser.Biography;
    user.BirthDate = newUser.BirthDate;

    await _context.SaveChangesAsync();

    return true;
  }

  public async Task<bool> DeleteUserAsync(int userId)
  {
    var user = await _context.Users.FindAsync(userId);
    if (user == null || user.DeletedAt != null)
      return false;

    user.DeletedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();
    return true;
  }

  public async Task<User?> GetUserByEmailAsync(string email, bool ignoreQueryFilters = false)
  {
    var query = _context.Users.AsQueryable();
    if (ignoreQueryFilters)
      query = query.IgnoreQueryFilters();
    return await query.FirstOrDefaultAsync(u => u.Email == email);
  }

  /// <summary>
  /// Atualiza a foto de perfil do usuário usando Azure Blob Storage.
  /// </summary>
  public async Task<string?> UpdateUserAvatarAsync(int userId, IFormFile avatarFile)
  {
    var user = await _context.Users.FindAsync(userId);
    if (user == null || avatarFile == null || avatarFile.Length == 0)
      return null;

    var connectionString = _azureSettings.BlobStorageConnectionString;
    var containerName = "users";
    var avatarsFolder = "avatars";
    var fileExt = Path.GetExtension(avatarFile.FileName);
    var fileName = $"{avatarsFolder}/{Guid.NewGuid()}{fileExt}";

    var blobServiceClient = new BlobServiceClient(connectionString);
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

    // Remove avatar antigo se existir
    if (!string.IsNullOrEmpty(user.AvatarURL))
    {
      try
      {
        var oldUri = new Uri(user.AvatarURL);
        var oldBlobName = string.Join("", oldUri.Segments.Skip(2)).TrimStart('/');
        var oldBlobClient = containerClient.GetBlobClient(oldBlobName);
        await oldBlobClient.DeleteIfExistsAsync();
      }
      catch
      {
        return null;
      }
    }

    var blobClient = containerClient.GetBlobClient(fileName);
    try
    {
      using var stream = avatarFile.OpenReadStream();
      var uploadOptions = new BlobUploadOptions
      {
        HttpHeaders = new BlobHttpHeaders { ContentType = avatarFile.ContentType },
      };
      await blobClient.UploadAsync(stream, uploadOptions, cancellationToken: default);
    }
    catch
    {
      return null;
    }

    user.AvatarURL = blobClient.Uri.ToString();
    await _context.SaveChangesAsync();
    return user.AvatarURL;
  }

  /// <summary>
  /// Remove a foto de perfil do usuário do Azure Blob Storage e limpa a coluna AvatarURL.
  /// </summary>
  public async Task<bool> RemoveUserAvatarAsync(int userId)
  {
    var user = await _context.Users.FindAsync(userId);
    if (user == null || string.IsNullOrEmpty(user.AvatarURL))
      return false;

    var connectionString = _azureSettings.BlobStorageConnectionString;
    var containerName = "users";
    var uri = new Uri(user.AvatarURL);
    var blobName = uri.Segments.Skip(2).Aggregate("", (a, b) => a + b).TrimStart('/'); // pega avatars/arquivo.ext

    var blobServiceClient = new BlobServiceClient(connectionString);
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    var blobClient = containerClient.GetBlobClient(blobName);
    await blobClient.DeleteIfExistsAsync();

    user.AvatarURL = null;
    await _context.SaveChangesAsync();
    return true;
  }
}
