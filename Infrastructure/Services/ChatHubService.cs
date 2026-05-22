using Microsoft.AspNetCore.SignalR;
using NexCommerce.Application.Contracts;
using NexCommerce.Infrastructure.Hubs;

namespace NexCommerce.Infrastructure.Services;

public sealed class ChatHubService(IHubContext<ChatHub> hubContext) : IChatHubService
{
    public async Task PushMessageAsync(Guid senderId, Guid receiverId, object messageDto, CancellationToken ct = default)
    {
        var conversationId = GetConversationId(senderId, receiverId);
        
        // Push message to the active conversation group
        await hubContext.Clients.Group(conversationId).SendAsync("ReceiveMessage", messageDto, ct);
        
        // Push a notification to the receiver's personal group
        await hubContext.Clients.Group(receiverId.ToString()).SendAsync("ReceiveMessageNotification", messageDto, ct);
    }

    private static string GetConversationId(Guid user1, Guid user2)
    {
        var list = new List<Guid> { user1, user2 };
        list.Sort();
        return $"{list[0]}_{list[1]}";
    }
}
