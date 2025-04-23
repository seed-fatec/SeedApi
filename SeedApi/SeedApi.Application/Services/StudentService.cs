using Microsoft.EntityFrameworkCore;
using SeedApi.Application.Interfaces;
using SeedApi.Domain.Entities;

namespace SeedApi.Application.Services;

public class StudentService(IPersistenceContext context)
{
  private readonly IPersistenceContext _context = context;

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
