using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeedApi.Application.DTOs.Requests.Users;
using SeedApi.Application.DTOs.Responses;
using SeedApi.Application.DTOs.Responses.Users;
using SeedApi.Application.Services;
using SeedApi.Domain.Entities;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SeedApi.API.Controllers;

[ApiController]
[Route("api/users/me")]
public sealed class UserProfileController(UserService userService) : ControllerBase
{
  private readonly UserService _userService = userService;

  private async Task<User?> GetAuthenticated()
  {
    var userIdClaim = User.FindFirstValue("UserId");
    var validId = int.TryParse(userIdClaim, out var userId);

    return validId ? await _userService.GetUserByIdAsync(userId) : null;
  }

  [HttpGet(Name = "GetCurrentUser")]
  [Authorize]
  [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GetCurrentUser()
  {
    var authenticatedUser = await GetAuthenticated();

    if (authenticatedUser == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var user = await _userService.GetUserByIdAsync(authenticatedUser.Id);

    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    return Ok(new UserResponse
    {
      Id = user.Id,
      Name = user.Name,
      Role = user.Role,
      BirthDate = user.BirthDate,
      Email = user.Email,
      CreatedAt = user.CreatedAt,
      UpdatedAt = user.UpdatedAt
    });
  }

  [HttpPut(Name = "UpdateCurrentUser")]
  [Authorize]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> UpdateCurrentUser([FromBody] UserUpdateRequest request)
  {
    var user = await GetAuthenticated();

    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var success = await _userService.UpdateUserAsync(user.Id, request);

    if (!success)
      return NotFound(new ErrorResponse { Message = "Usuário não encontrado." });

    return NoContent();
  }

  [HttpDelete(Name = "DeleteCurrentUser")]
  [Authorize]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeleteCurrentUser([FromBody] UserDeleteRequest request)
  {
    var user = await GetAuthenticated();

    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(request.Password));
    var requestPasswordHash = Convert.ToBase64String(hashedBytes);

    if (!string.Equals(requestPasswordHash, user.PasswordHash))
      return Unauthorized(new ErrorResponse { Message = "Senha incorreta." });

    await _userService.DeleteUserAsync(user.Id);

    return NoContent();
  }
}
