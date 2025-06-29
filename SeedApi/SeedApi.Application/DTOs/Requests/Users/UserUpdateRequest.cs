using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SeedApi.Application.DTOs.Requests.Users
{
  public record UserUpdateRequest
  {
    [Required(ErrorMessage = "O nome de usuário é obrigatório.")]
    [Description("O nome do usuário.")]
    public string Name { get; init; } = null!;

    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
    [Description("O e-mail do usuário.")]
    public string Email { get; init; } = null!;

    [Description("A bio do usuário.")]
    [MaxLength(length: 500, ErrorMessage = "A bio do usuário deve ter no máximo 500 caracteres")]
    public string? Biography { get; init; } = null!;

    [Description("A data de nascimento do usuário.")]
    [JsonPropertyName("birth_date")]
    public DateOnly? BirthDate { get; init; } = null!;
  }
}