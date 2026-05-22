using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Contracts.Repositories;

public interface INotificationRepository
{
    Task<(List<Notification> Items, int Total)> GetByUserPagedAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task<List<Notification>> GetByIdsForUserAsync(Guid userId, IEnumerable<Guid> ids, CancellationToken ct = default);
    Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
    void Add(Notification notification);
}
