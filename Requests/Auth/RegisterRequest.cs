using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SeedApi.Requests.Auth
{
  public record RegisterRequest
  {
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [Description("O nome do usuário.")]
    public string Name { get; init; } = null!;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
    [Description("O e-mail do usuário.")]
    public string Email { get; init; } = null!;

    [Required(ErrorMessage = "O formato da data de nascimento é inválido.")]
    [JsonPropertyName("birth_date")]
    [Description("A data de nascimento do usuário.")]
    public DateOnly BirthDate { get; init; }

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres.")]
    [Description("A senha do usuário.")]
    public string Password { get; init; } = null!;
  }
}