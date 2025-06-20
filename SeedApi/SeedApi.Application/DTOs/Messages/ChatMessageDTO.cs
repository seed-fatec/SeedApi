namespace SeedApi.Application.DTOs.Messages;

using System.ComponentModel.DataAnnotations;

public class ChatMessageDTO
{
  [Required(ErrorMessage = "A mensagem é obrigatória.")]
  [StringLength(2000, ErrorMessage = "A mensagem deve ter no máximo 2000 caracteres.")]
  public string Message { get; set; } = null!;
}
