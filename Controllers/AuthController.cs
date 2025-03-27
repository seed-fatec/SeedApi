using Microsoft.AspNetCore.Mvc;
using SeedApi.Requests.Auth;
using SeedApi.Responses;
using SeedApi.Responses.Auth;
using SeedApi.Models.Entities;
using SeedApi.Models.Config;
using SeedApi.Services;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SeedApi.Controllers;

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
      });
    }

    return CreatedAtAction(nameof(RegisterStudent), new RegisterResponse
    {
      Message = "Estudante registrado com sucesso.",
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
      });
    }

    return CreatedAtAction(nameof(RegisterTeacher), new RegisterResponse
    {
      Message = "Professor registrado com sucesso.",
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
      });
    }

    return Ok(new LoginResponse
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
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
      });
    }

    return Ok(new LoginResponse
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
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
      });
    }

    return Ok(new RefreshResponse
    {
      AccessToken = newAccessToken,
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
      });
    }

    return Ok(new LogoutResponse
    {
      Message = "Logout realizado com sucesso.",
    });
  }
}