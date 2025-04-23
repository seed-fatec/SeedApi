using Microsoft.EntityFrameworkCore;
using SeedApi.Application.Interfaces;
using SeedApi.Domain.Entities;

namespace SeedApi.Application.Services;

public class TeacherService(IPersistenceContext context)
{
  private readonly IPersistenceContext _context = context;

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

  public async Task<bool> IsTeacherInCourseAsync(int teacherId, int courseId)
  {
    return await _context.Courses
        .Where(c => c.Id == courseId)
        .AnyAsync(c => c.Teachers.Any(t => t.Id == teacherId));
  }
}
