using Microsoft.AspNetCore.Mvc;
using SeedApi.Requests.Auth;
using SeedApi.Responses;
using SeedApi.Responses.Auth;
using SeedApi.Services;

namespace SeedApi.Controllers
{
  [ApiController]
  [Route("api/auth")]
  public sealed class AuthController(AuthService authService) : ControllerBase
  {
    private readonly AuthService _authService = authService;

    [HttpPost("register", Name = "Register")]
    [ProducesResponseType<RegisterResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(new ErrorResponse
        {
          Message = "Dados de entrada inválidos.",
          Links =
          [
            new Link { Rel = "self", Href = Url.Link(nameof(Register), new {}) },
              new Link { Rel = "login", Href = Url.Link(nameof(Login), new {}) }
          ]
        });
      }

      var success = await _authService.RegisterAsync(request.Name, request.Email, request.BirthDate, request.Password);

      if (!success)
      {
        return BadRequest(new ErrorResponse
        {
          Message = "Usuário já existe.",
          Links =
          [
            new Link { Rel = "self", Href = Url.Link(nameof(Register), new {}) },
              new Link { Rel = "login", Href = Url.Link(nameof(Login), new {}) }
          ]
        });
      }

      return CreatedAtAction(nameof(Register), new RegisterResponse
      {
        Message = "Usuário registrado com sucesso.",
        Links =
        [
          new Link { Rel = "self", Href = Url.Link(nameof(Register), new {}) },
            new Link { Rel = "login", Href = Url.Link(nameof(Login), new {}) }
        ]
      });
    }

    [HttpPost("login", Name = "Login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(new ErrorResponse
        {
          Message = "Dados de entrada inválidos",
          Links =
          [
            new Link { Rel = "self", Href = Url.Link(nameof(Login), new {}) },
              new Link { Rel = "register", Href = Url.Link(nameof(Register), new {}) }
          ]
        });
      }

      var (accessToken, refreshToken) = await _authService.AuthenticateAsync(request.Email, request.Password);

      if (accessToken == null || refreshToken == null)
      {
        return Unauthorized(new ErrorResponse
        {
          Message = "Credenciais inválidas.",
          Links =
          [
            new Link { Rel = "self", Href = Url.Link(nameof(Login), new {}) },
              new Link { Rel = "register", Href = Url.Link(nameof(Register), new {}) }
          ]
        });
      }

      return Ok(new LoginResponse
      {
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        Links =
        [
          new Link { Rel = "self", Href = Url.Link(nameof(Login), new {}) },
          new Link { Rel = "refresh", Href = Url.Link(nameof(Refresh), new {}) },
          new Link { Rel = "logout", Href = Url.Link(nameof(Logout), new {}) },
        ]
      });
    }

    [HttpPost("refresh", Name = "Refresh")]
    [ProducesResponseType<RefreshResponse>(StatusCodes.Status200OK)]
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
            new Link { Rel = "login", Href = Url.Link(nameof(Login), new { }) }
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
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
      var success = await _authService.RevokeRefreshTokenAsync(request.RefreshToken);

      if (!success)
      {
        return BadRequest(new ErrorResponse
        {
          Message = "Falha ao revogar o Refresh Token.",
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
          new Link { Rel = "login", Href = Url.Link(nameof(Login), new { }) }
        ]
      });
    }
  }
}