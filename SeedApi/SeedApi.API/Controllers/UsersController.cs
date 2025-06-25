using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeedApi.API.Extensions;
using SeedApi.API.Middlewares;
using SeedApi.Application.DTOs.Responses;
using SeedApi.Application.DTOs.Responses.Users;
using SeedApi.Application.Services;

namespace SeedApi.API.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(UserService userService) : ControllerBase
{
  private readonly UserService _userService = userService;

  [HttpGet(Name = "GetUsers")]
  [Authorize]
  [AllowAdmin]
  [ProducesResponseType<UserCollectionResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetUsers()
  {
    if (!User.IsAdmin())
    {
      var authenticatedUser = await _userService.GetAuthenticatedUserAsync(User);

      if (authenticatedUser == null)
        return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });
    }

    var users = await _userService.GetAllUsersAsync();
    var safeUsers = users.Select(u => new PublicUserResponse
    {
      Id = u.Id,
      Name = u.Name,
      AvatarURL = u.AvatarURL,
      Biography = u.Biography,
      Role = u.Role,
      BirthDate = u.BirthDate,
    });

    return Ok(new UserCollectionResponse
    {
      Users = [.. safeUsers]
    });
  }

  [HttpGet("{id:int}", Name = "GetUser")]
  [Authorize]
  [AllowAdmin]
  [ProducesResponseType<PublicUserResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetUser(int id)
  {
    if (!User.IsAdmin())
    {
      var authenticatedUser = await _userService.GetAuthenticatedUserAsync(User);

      if (authenticatedUser == null)
        return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });
    }

    var user = await _userService.GetUserByIdAsync(id);

    if (user == null)
      return NotFound(new ErrorResponse { Message = "Usuário não encontrado." });

    return Ok(new PublicUserResponse
    {
      Id = user.Id,
      Name = user.Name,
      Role = user.Role,
      BirthDate = user.BirthDate,
      AvatarURL = user.AvatarURL,
      Biography = user.Biography
    });
  }
}
