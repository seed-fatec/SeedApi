using Microsoft.EntityFrameworkCore;
using SeedApi.Models;
using SeedApi.Models.Entities;
using SeedApi.Requests.Users;

namespace SeedApi.Services;

public class UserService(ApplicationDbContext context)
{
  private readonly ApplicationDbContext _context = context;

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
}
