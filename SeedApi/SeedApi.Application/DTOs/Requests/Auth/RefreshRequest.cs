using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SeedApi.Application.DTOs.Requests.Auth;

public record RefreshRequest
{
  [Required(ErrorMessage = "O refresh token é obrigatório.")]
  [JsonPropertyName("refresh_token")]
  [Description("O token de atualização.")]
  public string RefreshToken { get; init; } = null!;
};