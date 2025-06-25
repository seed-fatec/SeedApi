using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SeedApi.Application.DTOs.Requests.Auth;

public record LoginRequest
{
  [Required(ErrorMessage = "O e-mail é obrigatório.")]
  [Description("O email do usuário cadastrado.")]
  public string Email { get; init; } = null!;

  [Required(ErrorMessage = "A senha é obrigatória.")]
  [Description("A senha do usuário cadastrado.")]
  public string Password { get; init; } = null!;
}