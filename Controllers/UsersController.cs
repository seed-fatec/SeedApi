using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeedApi.Middlewares;
using SeedApi.Responses;
using SeedApi.Responses.Users;
using SeedApi.Services;
using System.Security.Claims;

namespace SeedApi.Controllers
{
  [ApiController]
  [Route("api/users")]
  public sealed class UsersController(UserService userService) : ControllerBase
  {
    private readonly UserService _userService = userService;

    [HttpGet("me", Name = "GetCurrentUser")]
    [Authorize]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
      var userId = User.FindFirstValue("UserId");

      if (userId == null)
      {
        return Unauthorized(new ErrorResponse
        {
          Message = "Usuário não autorizado.",
          Links =
          [
            new Link { Rel = "self", Href = Url.Link(nameof(GetCurrentUser), new { }) }
          ]
        });
      }

      var user = await _userService.GetUserByIdAsync(int.Parse(userId));

      if (user == null)
      {
        return Unauthorized(new ErrorResponse
        {
          Message = "Usuário não autorizado.",
          Links =
          [
            new Link { Rel = "self", Href = Url.Link(nameof(GetCurrentUser), new { }) }
          ]
        });
      }

      return Ok(new UserResponse
      {
        Id = user.Id.ToString(),
        Name = user.Name,
        Role = user.Role,
        BirthDate = user.BirthDate,
        Email = user.Email,
        Links =
        [
          new Link { Rel = "self", Href = Url.Link(nameof(GetCurrentUser), new { }) }
        ]
      });
    }
  }
}
