using Microsoft.EntityFrameworkCore;
using SeedApi.Models;
using SeedApi.Models.Entities;
using SeedApi.Requests.Courses;

namespace SeedApi.Services;

public class CourseService
{
  private readonly ApplicationDbContext _context;

  public CourseService(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<List<Course>> ListAllCoursesAsync()
  {
    return await _context.Courses.ToListAsync();
  }

  public async Task<Course?> GetCourseByIdAsync(int courseId)
  {
    return await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
  }

  public async Task<Course> AddCourseAsync(User teacher, Course newCourse)
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

    teacher.TaughtCourses.Add(course);
    await _context.SaveChangesAsync();

    return course;
  }

  public async Task<bool> RemoveCourseAsync(int courseId)
  {
    var course = await _context.Courses.FindAsync(courseId);
    if (course == null || course.DeletedAt != null)
      return false;

    _context.Courses.Remove(course);
    await _context.SaveChangesAsync();
    return true;
  }

  public async Task<List<Course>> ListCoursesByTeacherAsync(int teacherId)
  {
    var teacher = await _context.Users
      .Include(u => u.TaughtCourses)
      .FirstOrDefaultAsync(u => u.Id == teacherId);

    return teacher?.TaughtCourses.ToList() ?? [];
  }

  public async Task<bool> EnrollStudentInCourseAsync(int courseId, User student)
  {
    var course = await _context.Courses
      .Include(c => c.Students)
      .FirstOrDefaultAsync(c => c.Id == courseId);

    if (course == null)
      return false;

    course.Students.Add(student);
    await _context.SaveChangesAsync();

    return true;
  }

  public async Task<List<User>> GetStudentsByCourseIdAsync(int courseId)
  {
    var course = await _context.Courses
      .Include(c => c.Students)
      .FirstOrDefaultAsync(c => c.Id == courseId);

    return course?.Students.ToList() ?? [];
  }

  public async Task<List<User>> GetTeachersByCourseIdAsync(int courseId)
  {
    var course = await _context.Courses
      .Include(c => c.Teachers)
      .FirstOrDefaultAsync(c => c.Id == courseId);

    return course?.Teachers.ToList() ?? [];
  }
}
