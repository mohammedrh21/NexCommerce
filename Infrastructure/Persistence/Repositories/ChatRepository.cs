using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Repositories;

public sealed class ChatRepository(NexCommerceDbContext db) : IChatRepository
{
    public async Task<(List<ChatMessage> Items, int Total)> GetConversationPagedAsync(Guid userId, Guid partnerId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.ChatMessages
            .Where(m => (m.SenderId == userId && m.ReceiverId == partnerId) ||
                        (m.SenderId == partnerId && m.ReceiverId == userId))
            .OrderByDescending(m => m.SentAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<List<(Guid PartnerId, ChatMessage LastMessage, int UnreadCount)>> GetConversationSummariesAsync(Guid userId, CancellationToken ct = default)
    {
        var messages = await db.ChatMessages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync(ct);

        var summaries = new List<(Guid PartnerId, ChatMessage LastMessage, int UnreadCount)>();
        var grouped = messages.GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId);

        foreach (var group in grouped)
        {
            var partnerId = group.Key;
            var lastMessage = group.First();
            var unreadCount = 0; // Not supported by ChatMessage yet
            summaries.Add((partnerId, lastMessage, unreadCount));
        }

        return summaries;
    }

    public void Add(ChatMessage message) => db.ChatMessages.Add(message);
}
