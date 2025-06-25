using Microsoft.EntityFrameworkCore;
using SeedApi.Infrastructure.Persistence;
using SeedApi.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using SeedApi.Infrastructure.Config;

namespace SeedApi.Infrastructure.Seeders;

public class AdminSeeder(ApplicationDbContext context, Configuration configuration)
{
  private readonly ApplicationDbContext _context = context;
  private readonly Configuration _configuration = configuration;

  public async Task SeedAsync()
  {
    var email = _configuration.AdminSettings.Email;
    var password = _configuration.AdminSettings.Password;
    await RegisterOrUpdateAdminAsync(email, password);
  }

  private async Task RegisterOrUpdateAdminAsync(string email, string password)
  {
    var existingAdmin = await _context.Admins.IgnoreQueryFilters().FirstOrDefaultAsync();

    if (existingAdmin is not null)
    {
      existingAdmin.Email = email.ToLowerInvariant();
      existingAdmin.PasswordHash = HashPassword(password);

      _context.Admins.Update(existingAdmin);
      await _context.SaveChangesAsync();
      return;
    }

    var newAdmin = new Admin
    {
      Email = email,
      PasswordHash = HashPassword(password),
    };

    _context.Admins.Add(newAdmin);
    await _context.SaveChangesAsync();
  }

  private static string HashPassword(string password)
  {
    return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
  }
}
