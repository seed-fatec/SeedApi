using Microsoft.EntityFrameworkCore;
using SeedApi.Application.DTOs.Requests.Users;
using SeedApi.Application.Interfaces;
using SeedApi.Domain.Entities;
using System.Security.Claims;

namespace SeedApi.Application.Services;

public class UserService(IPersistenceContext context)
{
  private readonly IPersistenceContext _context = context;

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
}
