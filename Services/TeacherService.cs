using Microsoft.EntityFrameworkCore;
using SeedApi.Models;
using SeedApi.Models.Entities;

namespace SeedApi.Services;

public class TeacherService(ApplicationDbContext context)
{
  private readonly ApplicationDbContext _context = context;

  public async Task<List<User>> GetAllTeachersAsync()
  {
    return await _context.Users
      .Where(u => u.Role == UserRole.Teacher)
      .ToListAsync();
  }

  public async Task<User?> GetTeacherByIdAsync(int userId)
  {
    return await _context.Users
      .Where(u => u.Id == userId && u.Role == UserRole.Teacher)
      .FirstOrDefaultAsync();
  }
}
