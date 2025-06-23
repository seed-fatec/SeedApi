using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeedApi.Application.DTOs.Requests.Users;
using SeedApi.Application.DTOs.Responses;
using SeedApi.Application.DTOs.Responses.Users;
using SeedApi.Application.Services;
using SeedApi.Domain.Entities;
using System.ComponentModel.DataAnnotations;
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
      Biography = user.Biography,
      Role = user.Role,
      BirthDate = user.BirthDate,
      Email = user.Email,
      CreatedAt = user.CreatedAt,
      UpdatedAt = user.UpdatedAt,
      AvatarURL = user.AvatarURL
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

    var existingUser = await _userService.GetUserByEmailAsync(request.Email, ignoreQueryFilters: true);
    if (existingUser != null && existingUser.Id != user.Id)
      return Conflict(new ErrorResponse { Message = "E-mail já cadastrado para outro usuário." });

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

  [HttpPost("avatar")]
  [Authorize]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> UpdateAvatar([FromForm][Required(ErrorMessage = "O Arquivo é obrigatório.")] IFormFile file)
  {
    var user = await GetAuthenticated();
    if (user == null)
      return Unauthorized(new { message = "Usuário não autorizado." });

    if (file == null || file.Length == 0)
      return BadRequest(new { message = "Arquivo não enviado." });

    var permittedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
    if (!permittedTypes.Contains(file.ContentType))
      return BadRequest(new { message = "Arquivo deve ser uma imagem válida (jpeg, png, webp)." });

    try
    {
      var avatarUrl = await _userService.UpdateUserAvatarAsync(user.Id, file);
      if (avatarUrl == null)
        return StatusCode(500, new { message = "Falha ao atualizar avatar. Tente novamente mais tarde." });
      return Ok(new { avatarUrl });
    }
    catch
    {
      return StatusCode(500, new { message = "Erro inesperado ao atualizar avatar." });
    }
  }

  [HttpDelete("avatar")]
  [Authorize]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> RemoveAvatar()
  {
    var user = await GetAuthenticated();
    if (user == null)
      return Unauthorized(new { message = "Usuário não autorizado." });
    try
    {
      var result = await _userService.RemoveUserAvatarAsync(user.Id);
      if (!result)
        return BadRequest(new { message = "Usuário não possui avatar para remover." });
      return Ok();
    }
    catch
    {
      return StatusCode(500, new { message = "Erro inesperado ao remover avatar." });
    }
  }
}
