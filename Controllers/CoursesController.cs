using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeedApi.Extensions;
using SeedApi.Middlewares;
using SeedApi.Models.Entities;
using SeedApi.Requests.Courses;
using SeedApi.Responses;
using SeedApi.Responses.Courses;
using SeedApi.Responses.Users;
using SeedApi.Services;

namespace SeedApi.Controllers;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController(CourseService courseService, TeacherService teacherService, UserService userService) : ControllerBase
{
  private readonly CourseService _courseService = courseService;
  private readonly TeacherService _teacherService = teacherService;
  private readonly UserService _userService = userService;

  [HttpGet(Name = "ListCourses")]
  [Authorize]
  [AllowAdmin]
  [ProducesResponseType<CourseCollectionResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> ListCourses()
  {
    if (!User.IsAdmin())
    {
      var user = await _userService.GetAuthenticatedUserAsync(User);
      if (user == null)
        return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });
    }

    var courses = await _courseService.ListAllCoursesAsync();
    var response = new CourseCollectionResponse
    {
      Courses = [.. courses.Select(c => new CourseResponse
      {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        Price = c.Price,
        MaxCapacity = c.MaxCapacity,
        StartDate = c.StartDate,
        EndDate = c.EndDate
      })]
    };

    return Ok(response);
  }

  [HttpPost(Name = "CreateCourse")]
  [Authorize]
  [RequireRole(UserRole.Teacher)]
  [ProducesResponseType<CourseResponse>(StatusCodes.Status201Created)]
  [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> CreateCourse([FromBody] CourseCreateRequest request)
  {
    var user = await _userService.GetAuthenticatedUserAsync(User);
    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var newCourse = await _courseService.AddCourseAsync(user, new Course
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

  [HttpGet("{id:int}", Name = "GetCourse")]
  [Authorize]
  [AllowAdmin]
  [ProducesResponseType<CourseResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetCourse(int id)
  {
    if (!User.IsAdmin())
    {
      var user = await _userService.GetAuthenticatedUserAsync(User);
      if (user == null)
        return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });
    }

    var course = await _courseService.GetCourseByIdAsync(id);
    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });

    return Ok(new CourseResponse
    {
      Id = course.Id,
      Name = course.Name,
      Description = course.Description,
      Price = course.Price,
      MaxCapacity = course.MaxCapacity,
      StartDate = course.StartDate,
      EndDate = course.EndDate,
      CreatedAt = course.CreatedAt,
      UpdatedAt = course.UpdatedAt
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
    var user = await _userService.GetAuthenticatedUserAsync(User);
    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var course = await _courseService.GetCourseByIdAsync(id);
    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });

    var inCourse = await _teacherService.IsTeacherInCourseAsync(user.Id, course.Id);
    if (!inCourse)
      return Forbid();

    await _courseService.RemoveCourseAsync(course.Id);
    return NoContent();
  }

  [HttpPost("{id:int}/enroll", Name = "EnrollInCourse")]
  [Authorize]
  [RequireRole(UserRole.Student)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
  public async Task<IActionResult> EnrollInCourse(int id)
  {
    var user = await _userService.GetAuthenticatedUserAsync(User);
    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var course = await _courseService.GetCourseByIdAsync(id);
    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });

    var students = await _courseService.GetStudentsByCourseIdAsync(course.Id);
    if (students.Any(s => s.Id == user.Id))
      return Conflict(new ErrorResponse { Message = "Você já está matriculado neste curso." });

    if (course.MaxCapacity <= students.Count)
      return Conflict(new ErrorResponse { Message = "Não há vagas disponíveis neste curso." });

    var success = await _courseService.EnrollStudentInCourseAsync(id, user);
    if (!success)
      return BadRequest(new ErrorResponse { Message = "Não foi possível realizar a matrícula." });

    return Ok(new { Message = "Matrícula realizada com sucesso." });
  }

  [HttpGet("{id:int}/teachers", Name = "ListTeachers")]
  [Authorize]
  [AllowAdmin]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> ListTeachersByCourse(int id)
  {
    if (!User.IsAdmin())
    {
      var user = await _userService.GetAuthenticatedUserAsync(User);
      if (user == null)
        return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });
    }

    var course = await _courseService.GetCourseByIdAsync(id);
    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });

    var rawTeachers = await _courseService.GetTeachersByCourseIdAsync(course.Id);

    var teachers = rawTeachers.Select(t => new PublicUserResponse
    {
      Id = t.Id,
      Name = t.Name,
      Role = t.Role,
      BirthDate = t.BirthDate
    });

    return Ok(new UserCollectionResponse
    {
      Users = [.. teachers]
    });
  }

  [HttpGet("{id:int}/students", Name = "ListStudents")]
  [Authorize]
  [AllowAdmin]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> ListStudentsByCourse(int id)
  {
    if (!User.IsAdmin())
    {
      var user = await _userService.GetAuthenticatedUserAsync(User);
      if (user == null)
        return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });
    }

    var course = await _courseService.GetCourseByIdAsync(id);

    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });

    var rawStudents = await _courseService.GetStudentsByCourseIdAsync(course.Id);

    var students = rawStudents.Select(t => new PublicUserResponse
    {
      Id = t.Id,
      Name = t.Name,
      Role = t.Role,
      BirthDate = t.BirthDate
    });

    return Ok(new UserCollectionResponse
    {
      Users = [.. students]
    });
  }
}
