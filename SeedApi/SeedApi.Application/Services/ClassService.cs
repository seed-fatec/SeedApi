using Microsoft.EntityFrameworkCore;
using SeedApi.Application.Interfaces;
using SeedApi.Domain.Entities;

namespace SeedApi.Application.Services;

public class ClassService(IPersistenceContext context)
{
  private readonly IPersistenceContext _context = context;

  public async Task<Class?> GetClassByIdAsync(int classId)
  {
    return await _context.Classes
        .Include(c => c.Course)
        .FirstOrDefaultAsync(c => c.Id == classId);
  }

  public async Task<List<Class>> ListClassesByCourseAsync(int courseId)
  {
    return await _context.Classes
        .Where(c => c.CourseId == courseId)
        .ToListAsync();
  }

  public async Task<Class> AddClassAsync(Class newClass)
  {
    _context.Classes.Add(newClass);
    await _context.SaveChangesAsync();
    return newClass;
  }

  public async Task<bool> DeleteClassAsync(int classId)
  {
    var existing = await _context.Classes.FindAsync(classId);
    if (existing == null)
      return false;

    _context.Classes.Remove(existing);

    await _context.SaveChangesAsync();
    return true;
  }

  public async Task UpdateClassAsync(Class classEntity)
  {
    _context.Classes.Update(classEntity);
    await _context.SaveChangesAsync();
  }
}
