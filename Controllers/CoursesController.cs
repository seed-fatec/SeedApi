using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeedApi.Middlewares;
using SeedApi.Models.Entities;
using SeedApi.Requests.Courses;
using SeedApi.Responses;
using SeedApi.Responses.Courses;
using SeedApi.Services;
using System.Security.Claims;

namespace SeedApi.Controllers;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController(
  CourseService courseService,
  TeacherService teacherService,
  UserService userService
) : ControllerBase
{
  private readonly CourseService _courseService = courseService;
  private readonly TeacherService _teacherService = teacherService;
  private readonly UserService _userService = userService;

  private async Task<User?> GetAuthenticated()
  {
    var userIdClaim = User.FindFirstValue("UserId");
    var validId = int.TryParse(userIdClaim, out var userId);

    return validId ? await _userService.GetUserByIdAsync(userId) : null;
  }

  [HttpGet(Name = "GetCourses")]
  [Authorize]
  [ProducesResponseType<CourseCollectionResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetCourses()
  {
    var user = await GetAuthenticated();

    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var rawCourses = await _courseService.GetAllCoursesAsync();

    var courses = rawCourses.Select(c => new CourseResponse
    {
      Id = c.Id,
      Name = c.Name,
      Description = c.Name,
      Price = c.Price,
      MaxCapacity = c.MaxCapacity,
      StartDate = c.StartDate,
      EndDate = c.EndDate,
      CreatedAt = c.CreatedAt,
      UpdatedAt = c.UpdatedAt
    });

    return Ok(new CourseCollectionResponse
    {
      Courses = [.. courses]
    });
  }

  [HttpGet("{id:int}", Name = "GetCourse")]
  [Authorize]
  [ProducesResponseType<CourseResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetCourse(int id)
  {
    var user = await GetAuthenticated();

    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var course = await _courseService.GetCourseByIdAsync(id);

    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });

    return Ok(new CourseResponse
    {
      Id = course.Id,
      Name = course.Name,
      Description = course.Name,
      Price = course.Price,
      MaxCapacity = course.MaxCapacity,
      StartDate = course.StartDate,
      EndDate = course.EndDate,
      CreatedAt = course.CreatedAt,
      UpdatedAt = course.UpdatedAt
    });
  }

  [HttpPost(Name = "CreateCourse")]
  [Authorize]
  [RequireRole(UserRole.Teacher)]
  [ProducesResponseType<CourseResponse>(StatusCodes.Status201Created)]
  [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> CreateCourse([FromBody] CourseCreateRequest request)
  {
    var user = await GetAuthenticated();

    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var newCourse = await _courseService.CreateCourseAsync(user, new Course
    {
      Name = request.Name,
      Description = request.Description,
      Price = request.Price,
      MaxCapacity = request.MaxCapacity,
      StartDate = request.StartDate,
      EndDate = request.EndDate,
    });
    
    return CreatedAtAction(nameof(CreateCourse), new CourseResponse
    {
      Id = newCourse.Id,
      Name = newCourse.Name,
      Description = newCourse.Description,
      Price = newCourse.Price,
      MaxCapacity = newCourse.MaxCapacity,
      StartDate = newCourse.StartDate,
      EndDate = newCourse.EndDate,
      CreatedAt = newCourse.CreatedAt,
      UpdatedAt = newCourse.UpdatedAt
    });
  }

  [HttpDelete("{id:int}", Name = "DeleteCourse")]
  [Authorize]
  [RequireRole(UserRole.Teacher)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> DeleteCourse(int id)
  {
    var user = await GetAuthenticated();

    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var course = await _courseService.GetCourseByIdAsync(id);

    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });


    var inCourse = await _teacherService.IsTeacherInCourseAsync(user.Id, course.Id);

    if (!inCourse)
      return Forbid();

    await _courseService.DeleteCourseAsync(course.Id);

    return NoContent();
  }
}
