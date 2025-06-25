using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SeedApi.API.Hubs;
using SeedApi.Application.DTOs.Messages;
using SeedApi.Application.DTOs.Responses;
using SeedApi.Application.DTOs.Responses.Messages;
using SeedApi.Application.DTOs.Responses.Users;
using SeedApi.Application.Services;
using SeedApi.Domain.Entities;

namespace SeedApi.API.Controllers;

[ApiController]
[Route("api/users/{id}/chat")]
[Authorize]
public class ChatController(
    IHubContext<NotificationHub> hubContext,
    UserService userService,
    MessageService messageService
) : ControllerBase
{
  private readonly IHubContext<NotificationHub> _hubContext = hubContext;
  private readonly UserService _userService = userService;
  private readonly MessageService _messageService = messageService;

  [HttpPost]
  public async Task<IActionResult> SendMessage(int id, [FromBody] ChatMessageDTO dto)
  {
    var sender = await _userService.GetAuthenticatedUserAsync(User);
    if (sender == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    if (id == sender.Id)
      return BadRequest(new { error = "Você não pode enviar mensagem para si mesmo." });

    var recipient = await _userService.GetUserByIdAsync(id);
    if (recipient == null)
      return NotFound(new { error = "Usuário de destino não encontrado." });

    var senderId = sender.Id.ToString();
    if (!NotificationHub.UserConnections.TryGetValue(senderId, out var senderConnections) || senderConnections.Count == 0)
      return BadRequest(new { error = "Você não está conectado ao SignalR." });

    // Salva a mensagem no MongoDB
    var message = new Message
    {
      SenderId = sender.Id,
      RecipientId = id,
      Content = dto.Message,
      Timestamp = DateTime.UtcNow
    };
    await _messageService.AddMessageAsync(message);

    // Envia para todas as conexões do destinatário, se houver
    if (NotificationHub.UserConnections.TryGetValue(id.ToString(), out var recipientConnections))
    {
      foreach (var connId in recipientConnections)
      {
        await _hubContext.Clients.Client(connId).SendAsync("ReceiveMessage", new
        {
          sender = new PublicUserResponse
          {
            Id = sender.Id,
            AvatarURL = sender.AvatarURL,
            Biography = sender.Biography,
            BirthDate = sender.BirthDate,
            Name = sender.Name,
            Role = sender.Role
          },
          message
        });
      }
    }
    
    return Ok(new { Message = "Mensagem enviada." });
  }

  [HttpGet]
  public async Task<IActionResult> GetMessages(int id)
  {
    var user = await _userService.GetAuthenticatedUserAsync(User);
    if (user == null)
      return Unauthorized(new ErrorResponse { Message = "Usuário não autorizado." });

    if (user.Id == id)
      return BadRequest(new { error = "Você não pode buscar mensagens consigo mesmo." });

    var messages = await _messageService.GetMessagesBetweenUsersAsync(user.Id, id);
    var response = new MessageCollectionResponse { Messages = messages };
    return Ok(response);
  }
}
