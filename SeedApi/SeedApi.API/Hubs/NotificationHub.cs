using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SeedApi.Application.Services;
using System.Collections.Concurrent;

namespace SeedApi.API.Hubs
{
  [Authorize]
  public class NotificationHub(UserService userService) : Hub
  {
    private readonly UserService _userService = userService;
    public static readonly ConcurrentDictionary<string, HashSet<string>> UserConnections = new();

    public override async Task OnConnectedAsync()
    {
      var user = await _userService.GetAuthenticatedUserAsync(Context.User!);
      var userId = user?.Id.ToString();

      if (!string.IsNullOrEmpty(userId))
      {
        UserConnections.AddOrUpdate(userId,
          _ => [Context.ConnectionId],
          (_, set) => { set.Add(Context.ConnectionId); return set; });
      }
      await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
      var user = await _userService.GetAuthenticatedUserAsync(Context.User!);
      var userId = user?.Id.ToString();
      if (!string.IsNullOrEmpty(userId) && UserConnections.TryGetValue(userId, out var set))
      {
        set.Remove(Context.ConnectionId);
        if (set.Count == 0)
          UserConnections.TryRemove(userId, out _);
      }
      await base.OnDisconnectedAsync(exception);
    }
  }
}
