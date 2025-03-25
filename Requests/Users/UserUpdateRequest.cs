using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SeedApi.Requests.Users
{
  public record UserUpdateRequest
  {
    [Description("O nome do usuário.")]
    public string Name { get; init; } = null!;

    [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
    [Description("O e-mail do usuário.")]
    public string Email { get; init; } = null!;

    [Description("A data de nascimento do usuário.")]
    public DateOnly? BirthDate { get; init; } = null!;
  }
}