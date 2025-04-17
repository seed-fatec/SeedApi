using Microsoft.EntityFrameworkCore;
using SeedApi.Models;
using SeedApi.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace SeedApi.Seeders;

public class AdminSeeder(ApplicationDbContext context)
{
  private readonly ApplicationDbContext _context = context;

  public async Task SeedAsync()
  {
    await RegisterAdminAsync("admin@email.com", "admin");
  }

  private async Task RegisterAdminAsync(string email, string password)
  {
    var adminExists = await _context.Admins.IgnoreQueryFilters().AnyAsync(u => u.Email == email);

    if (adminExists)
      return;

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
