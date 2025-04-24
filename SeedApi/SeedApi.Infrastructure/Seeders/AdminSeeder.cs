using Microsoft.EntityFrameworkCore;
using SeedApi.Infrastructure.Persistence;
using SeedApi.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace SeedApi.Infrastructure.Seeders;

public class AdminSeeder(ApplicationDbContext context, IConfiguration configuration)
{
  private readonly ApplicationDbContext _context = context;
  private readonly IConfiguration _configuration = configuration;

  public async Task SeedAsync()
  {
    var email = _configuration["Admin:Email"] ?? "admin@email.com";
    var password = _configuration["Admin:Password"] ?? "admin";

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
