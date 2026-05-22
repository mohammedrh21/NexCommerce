using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace NexCommerce.Infrastructure.Hubs;

[Authorize]
public class ChatHub : Hub
{
    // Clients will call this from Javascript/Blazor to join a specific conversation
    public async Task JoinConversation(string conversationId)
    {
        // Add connection to the conversation group
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }
}
