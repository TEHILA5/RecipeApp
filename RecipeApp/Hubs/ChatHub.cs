using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace RecipeApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    { 
        private static readonly ConcurrentDictionary<string, string> _connectedUsers = new();

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name ?? "Anonymous";
            _connectedUsers[Context.ConnectionId] = userName;
             
            await Clients.All.SendAsync("UsersUpdated", _connectedUsers.Values.Distinct().ToList());
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            _connectedUsers.TryRemove(Context.ConnectionId, out _);
            await Clients.All.SendAsync("UsersUpdated", _connectedUsers.Values.Distinct().ToList());
            await base.OnDisconnectedAsync(ex);
        }
         
        public async Task SendPublicMessage(string message)
        {
            var userName = Context.User?.Identity?.Name ?? "Anonymous";
            await Clients.All.SendAsync("ReceivePublicMessage", new
            {
                sender = userName,
                message,
                timestamp = DateTime.UtcNow.ToString("HH:mm")
            });
        }
         
        public async Task SendPrivateMessage(string targetUserName, string message)
        {
            var senderName = Context.User?.Identity?.Name ?? "Anonymous";
             
            var targetConnections = _connectedUsers
                .Where(kv => kv.Value == targetUserName)
                .Select(kv => kv.Key)
                .ToList();

            if (!targetConnections.Any())
            {
                await Clients.Caller.SendAsync("ReceiveError", $"{targetUserName} is not online.");
                return;
            }

            var payload = new
            {
                sender = senderName,
                recipient = targetUserName, 
                message,
                timestamp = DateTime.UtcNow.ToString("HH:mm"),
                isPrivate = true
            };

            await Clients.Clients(targetConnections).SendAsync("ReceivePrivateMessage", payload);
            await Clients.Caller.SendAsync("ReceivePrivateMessage", payload);
        }
    }
}