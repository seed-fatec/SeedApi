using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeedApi.API.Extensions;
using SeedApi.API.Middlewares;
using SeedApi.Application.DTOs.Requests.Courses;
using SeedApi.Application.DTOs.Responses;
using SeedApi.Application.DTOs.Responses.Courses;
using SeedApi.Application.Services;
using SeedApi.Domain.Entities;
using SeedApi.Application.DTOs.Responses.Users;
using System.ComponentModel.DataAnnotations;

namespace SeedApi.API.Controllers;

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

    var courses = await _courseService.ListAllCoursesWithTeachersAsync();
    var response = new CourseCollectionResponse
    {
      Courses = [.. courses.Select(c => new CourseResponse
      {
        Id = c.course.Id,
        Name = c.course.Name,
        Description = c.course.Description,
        AvatarURL = c.course.AvatarURL,
        Price = c.course.Price,
        MaxCapacity = c.course.MaxCapacity,
        StartDate = c.course.StartDate,
        EndDate = c.course.EndDate,
        RemainingVacancies = (int)c.course.MaxCapacity - c.studentCount,
        CreatedAt = c.course.CreatedAt,
        UpdatedAt = c.course.UpdatedAt,
        Teachers = [.. c.teachers.Select(t => new PublicUserResponse
        {
          Id = t.Id,
          Name = t.Name,
          Biography = t.Biography,
          Role = t.Role,
          BirthDate = t.BirthDate,
          AvatarURL = t.AvatarURL
        })]
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
      AvatarURL = newCourse.AvatarURL,
      RemainingVacancies = (int)newCourse.MaxCapacity,
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

    var studentCount = await _courseService.GetStudentCountAsync(id);
    return Ok(new CourseResponse
    {
      Id = course.Id,
      Name = course.Name,
      Description = course.Description,
      Price = course.Price,
      MaxCapacity = course.MaxCapacity,
      StartDate = course.StartDate,
      EndDate = course.EndDate,
      RemainingVacancies = (int)course.MaxCapacity - studentCount,
      AvatarURL = course.AvatarURL,
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
      AvatarURL = t.AvatarURL,
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
      AvatarURL = t.AvatarURL,
      BirthDate = t.BirthDate
    });

    return Ok(new UserCollectionResponse
    {
      Users = [.. students]
    });
  }

  [HttpGet("taught")]
  [Authorize]
  [RequireRole(UserRole.Teacher)]
  [ProducesResponseType<CourseCollectionResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> ListTaughtCourses()
  {
    var user = await _userService.GetAuthenticatedUserAsync(User);
    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var courses = await _courseService.ListCoursesByTeacherAsync(user.Id);
    var response = new CourseCollectionResponse
    {
      Courses = [.. courses.Select(c => new CourseResponse
      {
        Id = c.course.Id,
        Name = c.course.Name,
        Description = c.course.Description,
        Price = c.course.Price,
        MaxCapacity = c.course.MaxCapacity,
        StartDate = c.course.StartDate,
        EndDate = c.course.EndDate,
        AvatarURL = c.course.AvatarURL,
        CreatedAt = c.course.CreatedAt,
        UpdatedAt = c.course.UpdatedAt,
        RemainingVacancies = (int)c.course.MaxCapacity - c.studentCount,
        Teachers = [.. c.course.Teachers.Select(t => new PublicUserResponse
        {
          Id = t.Id,
          Name = t.Name,
          Biography = t.Biography,
          Role = t.Role,
          BirthDate = t.BirthDate,
          AvatarURL = t.AvatarURL
        })]
      })]
    };
    return Ok(response);
  }

  [HttpGet("enrolled")]
  [Authorize]
  [RequireRole(UserRole.Student)]
  [ProducesResponseType<CourseCollectionResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> ListEnrolledCourses()
  {
    var user = await _userService.GetAuthenticatedUserAsync(User);
    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var enrolledCourses = await _courseService.ListCoursesByStudentAsync(user.Id);
    var response = new CourseCollectionResponse
    {
      Courses = [.. enrolledCourses.Select(c => new CourseResponse
      {
        Id = c.course.Id,
        Name = c.course.Name,
        Description = c.course.Description,
        Price = c.course.Price,
        MaxCapacity = c.course.MaxCapacity,
        StartDate = c.course.StartDate,
        EndDate = c.course.EndDate,
        AvatarURL = c.course.AvatarURL,
        CreatedAt = c.course.CreatedAt,
        UpdatedAt = c.course.UpdatedAt,
        Teachers = [.. c.course.Teachers.Select(t => new PublicUserResponse
        {
          Id = t.Id,
          Name = t.Name,
          Biography = t.Biography,
          Role = t.Role,
          BirthDate = t.BirthDate,
          AvatarURL = t.AvatarURL
        })]
      })]
    };
    return Ok(response);
  }

  [HttpPost("{courseId:int}/avatar")]
  [Authorize]
  [RequireRole(UserRole.Teacher)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> UpdateAvatar(int courseId, [FromForm][Required(ErrorMessage = "O Arquivo é obrigatório.")] IFormFile file)
  {
    var user = await _userService.GetAuthenticatedUserAsync(User);
    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var course = await _courseService.GetCourseByIdAsync(courseId);
    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });

    var isTeacher = await _teacherService.IsTeacherInCourseAsync(user.Id, courseId);
    if (!isTeacher)
      return Forbid();

    if (file == null || file.Length == 0)
      return BadRequest(new { message = "Arquivo não enviado." });

    var permittedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
    if (!permittedTypes.Contains(file.ContentType))
      return BadRequest(new { message = "Arquivo deve ser uma imagem válida (jpeg, png, webp)." });

    try
    {
      var avatarUrl = await _courseService.UpdateCourseAvatarAsync(course.Id, file);
      if (avatarUrl == null)
        return StatusCode(500, new { message = "Falha ao atualizar avatar. Tente novamente mais tarde." });
      return Ok(new { avatarUrl });
    }
    catch
    {
      return StatusCode(500, new { message = "Erro inesperado ao atualizar avatar." });
    }
  }

  [HttpDelete("{courseId:int}/avatar")]
  [Authorize]
  [RequireRole(UserRole.Teacher)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> RemoveAvatar(int courseId)
  {
    var user = await _userService.GetAuthenticatedUserAsync(User);
    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var course = await _courseService.GetCourseByIdAsync(courseId);
    if (course == null)
      return NotFound(new ErrorResponse { Message = "Curso não encontrado." });

    var isTeacher = await _teacherService.IsTeacherInCourseAsync(user.Id, courseId);
    if (!isTeacher)
      return Forbid();

    try
    {
      var result = await _courseService.RemoveCourseAvatarAsync(course.Id);
      if (!result)
        return BadRequest(new { message = "Curso não possui avatar para remover." });
      return Ok();
    }
    catch
    {
      return StatusCode(500, new { message = "Erro inesperado ao remover avatar." });
    }
  }
}
