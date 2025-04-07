using Microsoft.EntityFrameworkCore;
using SeedApi.Models;
using SeedApi.Models.Entities;
using SeedApi.Requests.Courses;

namespace SeedApi.Services;

public class CourseService(ApplicationDbContext context)
{
  private readonly ApplicationDbContext _context = context;

  public async Task<List<Course>> GetAllCoursesAsync()
  {
    return await _context.Courses
      .ToListAsync();
  }

  public async Task<Course?> GetCourseByIdAsync(int courseId)
  {
    return await _context.Courses
      .Where(u => u.Id == courseId)
      .FirstOrDefaultAsync();
  }

  public async Task<Course> CreateCourseAsync(User user, Course newCourse)
  {
    var course = new Course
    {
      Name = newCourse.Name,
      Description = newCourse.Description,
      Price = newCourse.Price,
      MaxCapacity = newCourse.MaxCapacity,
      StartDate = newCourse.StartDate,
      EndDate = newCourse.EndDate
    };

    user.TaughtCourses.Add(course);

    await _context.SaveChangesAsync();

    return course;
  }

  public async Task<bool> DeleteCourseAsync(int courseId)
  {
    var course = await _context.Courses.FindAsync(courseId);
    if (course == null || course.DeletedAt != null)
      return false;

    course.DeletedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();
    return true;
  }

  public async Task<List<Course>> GetCoursesByTeacherAsync(int teacherId)
  {
    var user = await _context.Users
        .Include(u => u.TaughtCourses)
        .FirstOrDefaultAsync(u => u.Id == teacherId);

    if (user == null)
    {
      return [];
    }

    return [.. user.TaughtCourses];
  }
}
