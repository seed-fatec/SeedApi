using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeedApi.Models.Entities;
using SeedApi.Responses;
using SeedApi.Responses.Users;
using SeedApi.Services;
using System.Security.Claims;

namespace SeedApi.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(UserService userService) : ControllerBase
{
  private readonly UserService _userService = userService;

  private async Task<User?> GetAuthenticated()
  {
    var userIdClaim = User.FindFirstValue("UserId");
    var validId = int.TryParse(userIdClaim, out var userId);

    return validId ? await _userService.GetUserByIdAsync(userId) : null;
  }

  [HttpGet(Name = "GetUsers")]
  [Authorize]
  [ProducesResponseType<UserCollectionResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetUsers()
  {
    var authenticatedUser = await GetAuthenticated();

    if (authenticatedUser == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var users = await _userService.GetAllUsersAsync();
    var safeUsers = users.Select(u => new PublicUserResponse
    {
      Id = u.Id,
      Name = u.Name,
      BirthDate = u.BirthDate
    });

    return Ok(new UserCollectionResponse
    {
      Users = [.. safeUsers]
    });
  }

  [HttpGet("{id:int}", Name = "GetUser")]
  [Authorize]
  [ProducesResponseType<PublicUserResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetUser(int id)
  {
    var authenticatedUser = await GetAuthenticated();

    if (authenticatedUser == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var user = await _userService.GetUserByIdAsync(id);

    if (user == null)
      return NotFound(new ErrorResponse { Message = "Usuário não encontrado." });

    return Ok(new PublicUserResponse
    {
      Id = user.Id,
      Name = user.Name,
      Role = user.Role,
      BirthDate = user.BirthDate,
    });
  }
}
