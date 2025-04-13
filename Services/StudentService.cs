using Microsoft.EntityFrameworkCore;
using SeedApi.Models;
using SeedApi.Models.Entities;

namespace SeedApi.Services;

public class StudentService(ApplicationDbContext context)
{
  private readonly ApplicationDbContext _context = context;

  public async Task<List<User>> GetAllStudentsAsync()
  {
    return await _context.Users
      .Where(u => u.Role == UserRole.Student)
      .ToListAsync();
  }

  public async Task<User?> GetStudentByIdAsync(int userId)
  {
    return await _context.Users
      .Where(u => u.Id == userId && u.Role == UserRole.Student)
      .FirstOrDefaultAsync();
  }

  public async Task<bool> IsStudentInCourseAsync(int studentId, int courseId)
  {
    return await _context.Courses
        .Where(c => c.Id == courseId)
        .Include(c => c.Students)
        .AnyAsync(c => c.Students.Any(t => t.Id == studentId));
  }
}
