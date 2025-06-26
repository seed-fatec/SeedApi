using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeedApi.API.Extensions;
using SeedApi.API.Middlewares;
using SeedApi.Application.DTOs.Responses;
using SeedApi.Application.DTOs.Responses.Users;
using SeedApi.Application.Services;

namespace SeedApi.API.Controllers;

[ApiController]
[Route("api/students")]
public sealed class StudentsController(UserService userService, StudentService studentService) : ControllerBase
{
  private readonly UserService _userService = userService;
  private readonly StudentService _studentService = studentService;

  [HttpGet(Name = "GetStudents")]
  [Authorize]
  [AllowAdmin]
  [ProducesResponseType<UserCollectionResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetStudents()
  {
    if (!User.IsAdmin())
    {
      var authenticatedUser = await _userService.GetAuthenticatedUserAsync(User);

      if (authenticatedUser == null)
        return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });
    }

    var students = await _studentService.GetAllStudentsAsync();

    if (User.IsAdmin())
    {
      var studentList = students.Select(s => new UserResponse
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
      return Ok(new { Users = studentList });
    }

    var safeStudents = students.Select(s => new PublicUserResponse
    {
      Id = s.Id,
      Name = s.Name,
      AvatarURL = s.AvatarURL,
      Biography = s.Biography,
      Role = s.Role,
      BirthDate = s.BirthDate,
    });

    return Ok(new UserCollectionResponse
    {
      Users = [.. safeStudents]
    });
  }

  [HttpGet("{id:int}", Name = "GetStudent")]
  [Authorize]
  [AllowAdmin]
  [ProducesResponseType<PublicUserResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetStudent(int id)
  {
    if (!User.IsAdmin())
    {
      var authenticatedUser = await _userService.GetAuthenticatedUserAsync(User);

      if (authenticatedUser == null)
        return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });
    }

    var student = await _studentService.GetStudentByIdAsync(id);

    if (student == null)
      return NotFound(new ErrorResponse { Message = "Usuário não encontrado." });

    if (User.IsAdmin())
    {
      return Ok(new UserResponse
      {
        Id = student.Id,
        Name = student.Name,
        Role = student.Role,
        BirthDate = student.BirthDate,
        AvatarURL = student.AvatarURL,
        Biography = student.Biography,
        Email = student.Email,
        CreatedAt = student.CreatedAt,
        UpdatedAt = student.UpdatedAt
      });
    }

    return Ok(new PublicUserResponse
    {
      Id = student.Id,
      Name = student.Name,
      Role = student.Role,
      BirthDate = student.BirthDate,
      AvatarURL = student.AvatarURL,
      Biography = student.Biography
    });
  }
}
