using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeedApi.API.Extensions;
using SeedApi.API.Middlewares;
using SeedApi.Application.DTOs.Responses;
using SeedApi.Application.DTOs.Responses.Users;
using SeedApi.Application.Services;

namespace SeedApi.API.Controllers;

[ApiController]
[Route("api/teachers")]
public sealed class TeachersController(UserService userService, TeacherService teacherService) : ControllerBase
{
  private readonly UserService _userService = userService;
  private readonly TeacherService _teacherService = teacherService;

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
    
    var safeTeachers = teachers.Select(s => new PublicUserResponse
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
}
