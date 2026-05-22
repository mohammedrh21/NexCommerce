using NexCommerce.Domain.Enums;

namespace NexCommerce.Application.DTOs.Notifications;

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Message,
    bool IsRead,
    DateTime CreatedAt);

public record MarkNotificationsReadRequest(IEnumerable<Guid> NotificationIds);
