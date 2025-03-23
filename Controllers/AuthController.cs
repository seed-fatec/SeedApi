using Microsoft.AspNetCore.Mvc;
using SeedApi.Requests.Auth;
using SeedApi.Responses;
using SeedApi.Responses.Auth;
using SeedApi.Models;
using SeedApi.Services;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SeedApi.Controllers
{
  [ApiController]
  [Route("api")]
  public sealed class AuthController(AuthService authService, AdminSettings adminSettings) : ControllerBase
  {
    private readonly AuthService _authService = authService;
    private readonly AdminSettings _adminSettings = adminSettings;

    [HttpPost("student/register", Name = "RegisterStudent")]
    [ProducesResponseType<RegisterResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterStudent([FromBody] RegisterRequest request)
    {
      var success = await _authService.RegisterAsync(request.Name, request.Email, request.Password, UserRole.Student);

      if (!success)
      {
        return Conflict(new ErrorResponse
        {
          Message = "Usuário já existe.",
          Links =
          [
            new Link { Rel = "self", Href = Url.Link(nameof(RegisterStudent), new {}) },
            new Link { Rel = "login", Href = Url.Link(nameof(LoginStudent), new {}) }
          ]
        });
      }

      return CreatedAtAction(nameof(RegisterStudent), new RegisterResponse
      {
        Message = "Estudante registrado com sucesso.",
        Links =
      [
        new Link { Rel = "self", Href = Url.Link(nameof(RegisterStudent), new {}) },
        new Link { Rel = "login", Href = Url.Link(nameof(LoginStudent), new {}) }
      ]
      });
    }

    [HttpPost("teacher/register", Name = "RegisterTeacher")]
    [ProducesResponseType<RegisterResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterTeacher(
      [FromHeader(Name = "X-Admin-Key")]
      [Description("Chave de administrador do sistema.")]
      [Required(ErrorMessage = "A chave de adminstrador é obrigatória.")]
      string adminKey,
      [FromBody] RegisterRequest request
    )
    {
      if (adminKey != _adminSettings.Secret)
      {
        return Forbid();
      }

      var success = await _authService.RegisterAsync(request.Name, request.Email, request.Password, UserRole.Teacher);

      if (!success)
      {
        return Conflict(new ErrorResponse
        {
          Message = "Usuário já existe.",
          Links =
          [
            new Link { Rel = "self", Href = Url.Link(nameof(RegisterTeacher), new {}) },
            new Link { Rel = "login", Href = Url.Link(nameof(LoginTeacher), new {}) }
          ]
        });
      }

      return CreatedAtAction(nameof(RegisterTeacher), new RegisterResponse
      {
        Message = "Professor registrado com sucesso.",
        Links =
        [
          new Link { Rel = "self", Href = Url.Link(nameof(RegisterTeacher), new {}) },
          new Link { Rel = "login", Href = Url.Link(nameof(LoginTeacher), new {}) }
        ]
      });
    }

    [HttpPost("student/login", Name = "LoginStudent")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginStudent([FromBody] LoginRequest request)
    {
      var (accessToken, refreshToken) = await _authService.AuthenticateAsync(request.Email, request.Password, UserRole.Student);

      if (accessToken == null || refreshToken == null)
      {
        return Unauthorized(new ErrorResponse
        {
          Message = "Credenciais inválidas.",
          Links =
          [
            new Link { Rel = "self", Href = Url.Link(nameof(LoginStudent), new {}) },
            new Link { Rel = "register", Href = Url.Link(nameof(RegisterStudent), new {}) }
          ]
        });
      }

      return Ok(new LoginResponse
      {
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        Links =
        [
          new Link { Rel = "self", Href = Url.Link(nameof(LoginStudent), new {}) },
          new Link { Rel = "refresh", Href = Url.Link(nameof(Refresh), new {}) },
          new Link { Rel = "logout", Href = Url.Link(nameof(Logout), new {}) },
        ]
      });
    }

    [HttpPost("teacher/login", Name = "LoginTeacher")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginTeacher([FromBody] LoginRequest request)
    {
      var (accessToken, refreshToken) = await _authService.AuthenticateAsync(request.Email, request.Password, UserRole.Teacher);

      if (accessToken == null || refreshToken == null)
      {
        return Unauthorized(new ErrorResponse
        {
          Message = "Credenciais inválidas.",
          Links =
          [
            new Link { Rel = "self", Href = Url.Link(nameof(LoginTeacher), new {}) },
            new Link { Rel = "register", Href = Url.Link(nameof(RegisterTeacher), new {}) }
          ]
        });
      }

      return Ok(new LoginResponse
      {
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        Links =
        [
          new Link { Rel = "self", Href = Url.Link(nameof(LoginTeacher), new {}) },
          new Link { Rel = "refresh", Href = Url.Link(nameof(Refresh), new {}) },
          new Link { Rel = "logout", Href = Url.Link(nameof(Logout), new {}) },
        ]
      });
    }

    [HttpPost("token/refresh", Name = "Refresh")]
    [ProducesResponseType<RefreshResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<RefreshResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
      var newAccessToken = await _authService.RefreshAccessTokenAsync(request.RefreshToken);

      if (newAccessToken == null)
      {
        return Unauthorized(new ErrorResponse
        {
          Message = "Refresh Token inválido ou expirado.",
          Links =
          [
            new Link { Rel = "self", Href = Url.Link(nameof(Refresh), new { }) },
          ]
        });
      }

      return Ok(new RefreshResponse
      {
        AccessToken = newAccessToken,
        Links =
        [
          new Link { Rel = "self", Href = Url.Link(nameof(Refresh), new { }) },
          new Link { Rel = "logout", Href = Url.Link(nameof(Logout), new { }) }
        ]
      });
    }

    [HttpPost("logout", Name = "Logout")]
    [ProducesResponseType<LogoutResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
      var success = await _authService.RevokeRefreshTokenAsync(request.RefreshToken);

      if (!success)
      {
        return Unauthorized(new ErrorResponse
        {
          Message = "Refresh token inválido.",
          Links =
          [
            new Link { Rel = "self", Href = Url.Link(nameof(Logout), new { }) }
          ]
        });
      }

      return Ok(new LogoutResponse
      {
        Message = "Logout realizado com sucesso.",
        Links =
        [
          new Link { Rel = "self", Href = Url.Link(nameof(Logout), new { }) },
        ]
      });
    }
  }
}