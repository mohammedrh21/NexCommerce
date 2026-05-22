using NexCommerce.Application.Common;
using NexCommerce.Application.DTOs.Chat;

namespace NexCommerce.Application.Contracts;

public interface IChatService
{
    Task<PagedResult<ChatMessageDto>> GetConversationAsync(Guid userId, Guid partnerId, int page, int pageSize, CancellationToken ct = default);
    Task<IEnumerable<ChatConversationDto>> GetConversationsAsync(Guid userId, CancellationToken ct = default);
    Task<ChatMessageDto> SendMessageAsync(Guid senderId, SendChatMessageRequest request, CancellationToken ct = default);
}
