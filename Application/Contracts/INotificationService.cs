using NexCommerce.Application.Common;
using NexCommerce.Application.DTOs.Notifications;
using NexCommerce.Domain.Enums;

namespace NexCommerce.Application.Contracts;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid userId, IEnumerable<Guid> notificationIds, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
    Task SendAsync(Guid userId, NotificationType type, string message, CancellationToken ct = default);
    Task SendToAllAsync(NotificationType type, string message, CancellationToken ct = default);
}
