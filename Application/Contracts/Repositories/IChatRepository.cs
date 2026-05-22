using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Contracts.Repositories;

public interface IChatRepository
{
    Task<(List<ChatMessage> Items, int Total)> GetConversationPagedAsync(
        Guid userId, Guid partnerId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns last message per conversation partner for inbox view.</summary>
    Task<List<(Guid PartnerId, ChatMessage LastMessage, int UnreadCount)>> GetConversationSummariesAsync(
        Guid userId, CancellationToken ct = default);

    void Add(ChatMessage message);
}
