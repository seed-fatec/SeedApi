using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeedApi.API.Extensions;
using SeedApi.API.Middlewares;
using SeedApi.Application.DTOs.Responses;
using SeedApi.Application.DTOs.Responses.Courses;
using SeedApi.Application.DTOs.Responses.Users;
using SeedApi.Application.Services;

namespace SeedApi.API.Controllers;

[ApiController]
[Route("api/teachers")]
public sealed class TeachersController(UserService userService, TeacherService teacherService, CourseService courseService) : ControllerBase
{
  private readonly UserService _userService = userService;
  private readonly TeacherService _teacherService = teacherService;
  private readonly CourseService _courseService = courseService;

  [HttpGet(Name = "GetTeachers")]
  [Authorize]
  [AllowAdmin]
  [ProducesResponseType<UserCollectionResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetTeachers()
  {
    if (!User.IsAdmin())
    {
      var authenticatedUser = await _userService.GetAuthenticatedUserAsync(User);

      if (authenticatedUser == null)
        return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });
    }

    var teachers = await _teacherService.GetAllTeachersAsync();

    if (User.IsAdmin())
    {
      var teacherList = teachers.Select(s => new UserResponse
      {
        Id = s.Id,
        Name = s.Name,
        AvatarURL = s.AvatarURL,
        Biography = s.Biography,
        Role = s.Role,
        BirthDate = s.BirthDate,
        Email = s.Email,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
      });

      // Retorna um objeto anônimo para não quebrar o contrato do UserCollectionResponse
      return Ok(new { Users = teacherList });
    }

    var safeTeachers = teachers.Select(t => new PublicUserResponse
    {
      Id = t.Id,
      Name = t.Name,
      AvatarURL = t.AvatarURL,
      Biography = t.Biography,
      Role = t.Role,
      BirthDate = t.BirthDate
    });

    return Ok(new UserCollectionResponse
    {
      Users = [.. safeTeachers]
    });
  }

  [HttpGet("{id:int}", Name = "GetTeacher")]
  [Authorize]
  [AllowAdmin]
  [ProducesResponseType<PublicUserResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetTeacher(int id)
  {
    if (!User.IsAdmin())
    {
      var authenticatedUser = await _userService.GetAuthenticatedUserAsync(User);

      if (authenticatedUser == null)
        return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });
    }

    var teacher = await _teacherService.GetTeacherByIdAsync(id);

    if (teacher == null)
      return NotFound(new ErrorResponse { Message = "Usuário não encontrado." });

    if (User.IsAdmin())
    {
      return Ok(new UserResponse
      {
        Id = teacher.Id,
        Name = teacher.Name,
        Role = teacher.Role,
        BirthDate = teacher.BirthDate,
        AvatarURL = teacher.AvatarURL,
        Biography = teacher.Biography,
        Email = teacher.Email,
        CreatedAt = teacher.CreatedAt,
        UpdatedAt = teacher.UpdatedAt
      });
    }

    return Ok(new PublicUserResponse
    {
      Id = teacher.Id,
      Name = teacher.Name,
      Role = teacher.Role,
      BirthDate = teacher.BirthDate,
      AvatarURL = teacher.AvatarURL,
      Biography = teacher.Biography
    });
  }

  [HttpGet("{id:int}/courses")]
  [Authorize]
  [AllowAdmin]
  [ProducesResponseType<CourseCollectionResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetTeacherCourses(int id)
  {
    var teacher = await _teacherService.GetTeacherByIdAsync(id);
    if (teacher == null)
      return NotFound(new ErrorResponse { Message = "Usuário não encontrado." });

    var courses = await _courseService.ListCoursesByTeacherAsync(id);
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
        RemainingVacancies = (int)c.course.MaxCapacity - c.studentCount,
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
}
