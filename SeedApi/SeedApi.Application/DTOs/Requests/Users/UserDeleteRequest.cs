using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SeedApi.Application.DTOs.Requests.Users
{
  public record UserDeleteRequest
  {
    [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
    [Description("A senha do usuário para confirmar a exclusão da conta.")]
    public string Password { get; init; } = null!;
  }
}