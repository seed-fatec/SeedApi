using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SeedApi.Application.Interfaces;
using SeedApi.Domain.Configuration;
using SeedApi.Domain.Entities;

namespace SeedApi.Application.Services;

public class CourseService(IPersistenceContext context, IOptions<AzureSettings> azureSettings)
{
  private readonly IPersistenceContext _context = context;
  private readonly AzureSettings _azureSettings = azureSettings.Value;

  public async Task<List<(Course course, int studentCount)>> ListAllCoursesAsync()
  {
    var coursesWithCounts = await _context.Courses
      .Select(c => new { Course = c, StudentCount = c.Students.Count })
      .ToListAsync();

    return [.. coursesWithCounts.Select(x => (x.Course, x.StudentCount))];
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

  public async Task<List<Course>> ListCoursesByStudentAsync(int studentId)
  {
    var student = await _context.Users
      .Include(u => u.EnrolledCourses)
      .FirstOrDefaultAsync(u => u.Id == studentId);

    return student?.EnrolledCourses.ToList() ?? [];
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

  public async Task<int> GetStudentCountAsync(int courseId)
  {
    return await _context.Courses
      .Where(c => c.Id == courseId)
      .SelectMany(c => c.Students)
      .CountAsync();
  }

  /// <summary>
  /// Atualiza a foto de perfil do usuário usando Azure Blob Storage.
  /// </summary>
  public async Task<string?> UpdateCourseAvatarAsync(int courseId, IFormFile avatarFile)
  {
    var course = await _context.Courses.FindAsync(courseId);
    if (course == null || avatarFile == null || avatarFile.Length == 0)
      return null;

    var connectionString = _azureSettings.BlobStorageConnectionString;
    var containerName = "courses";
    var avatarsFolder = "avatars";
    var fileExt = Path.GetExtension(avatarFile.FileName);
    var fileName = $"{avatarsFolder}/{Guid.NewGuid()}{fileExt}";

    var blobServiceClient = new BlobServiceClient(connectionString);
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

    // Remove avatar antigo se existir
    if (!string.IsNullOrEmpty(course.AvatarURL))
    {
      try
      {
        var oldUri = new Uri(course.AvatarURL);
        var oldBlobName = string.Join("", oldUri.Segments.Skip(2)).TrimStart('/');
        var oldBlobClient = containerClient.GetBlobClient(oldBlobName);
        await oldBlobClient.DeleteIfExistsAsync();
      }
      catch
      {
        return null;
      }
    }

    var blobClient = containerClient.GetBlobClient(fileName);
    try
    {
      using var stream = avatarFile.OpenReadStream();
      var uploadOptions = new BlobUploadOptions
      {
        HttpHeaders = new BlobHttpHeaders { ContentType = avatarFile.ContentType },
      };
      await blobClient.UploadAsync(stream, uploadOptions, cancellationToken: default);
    }
    catch
    {
      return null;
    }

    course.AvatarURL = blobClient.Uri.ToString();
    await _context.SaveChangesAsync();
    return course.AvatarURL;
  }

  /// <summary>
  /// Remove a foto de perfil do usuário do Azure Blob Storage e limpa a coluna AvatarURL.
  /// </summary>
  public async Task<bool> RemoveCourseAvatarAsync(int courseId)
  {
    var course = await _context.Courses.FindAsync(courseId);
    if (course == null || string.IsNullOrEmpty(course.AvatarURL))
      return false;

    var connectionString = _azureSettings.BlobStorageConnectionString;
    var containerName = "courses";
    var uri = new Uri(course.AvatarURL);
    var blobName = uri.Segments.Skip(2).Aggregate("", (a, b) => a + b).TrimStart('/'); // pega avatars/arquivo.ext

    var blobServiceClient = new BlobServiceClient(connectionString);
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    var blobClient = containerClient.GetBlobClient(blobName);
    await blobClient.DeleteIfExistsAsync();

    course.AvatarURL = null;
    await _context.SaveChangesAsync();
    return true;
  }
}
