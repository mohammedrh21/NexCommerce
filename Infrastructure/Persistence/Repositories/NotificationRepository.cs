using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository(NexCommerceDbContext db) : INotificationRepository
{
    public async Task<(List<Notification> Items, int Total)> GetByUserPagedAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task<List<Notification>> GetByIdsForUserAsync(Guid userId, IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await db.Notifications
            .Where(n => n.UserId == userId && ids.Contains(n.Id))
            .ToListAsync(ct);
    }

    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
    {
        var unread = await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }
    }

    public void Add(Notification notification) => db.Notifications.Add(notification);
}
