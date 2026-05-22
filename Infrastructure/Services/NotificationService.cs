using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Notifications;
using NexCommerce.Domain.Entities;
using NexCommerce.Domain.Enums;
using NexCommerce.Infrastructure.Hubs;

namespace NexCommerce.Infrastructure.Services;

/// <summary>
/// Notification service that persists notifications to the database and pushes
/// real-time updates to connected clients via SignalR.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repository,
        IUnitOfWork unitOfWork,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _logger     = logger;
    }

    // ── Queries ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await _repository.GetByUserPagedAsync(userId, page, pageSize, ct);

        return new PagedResult<NotificationDto>
        {
            Data       = items.Select(MapToDto),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
        };
    }

    /// <inheritdoc/>
    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
        => _repository.GetUnreadCountAsync(userId, ct);

    // ── Mutations ──────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task MarkAsReadAsync(
        Guid userId, IEnumerable<Guid> notificationIds, CancellationToken ct = default)
    {
        var notifications = await _repository.GetByIdsForUserAsync(userId, notificationIds, ct);

        foreach (var n in notifications)
            n.IsRead = true;

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogDebug("Marked {Count} notification(s) as read for user {UserId}",
            notifications.Count, userId);
    }

    /// <inheritdoc/>
    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        await _repository.MarkAllReadAsync(userId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogDebug("Marked all notifications as read for user {UserId}", userId);
    }

    // ── Real-time send ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task SendAsync(
        Guid userId, NotificationType type, string message, CancellationToken ct = default)
    {
        // 1. Persist
        var notification = new Notification
        {
            Id        = Guid.NewGuid(),
            UserId    = userId,
            Type      = type,
            Message   = message,
            IsRead    = false,
            CreatedAt = DateTime.UtcNow,
        };

        _repository.Add(notification);
        await _unitOfWork.SaveChangesAsync(ct);

        // 2. Push via SignalR to the user's personal group
        var dto = MapToDto(notification);
        await _hubContext.Clients
            .Group(userId.ToString())
            .SendAsync("ReceiveNotification", dto, ct);

        _logger.LogInformation(
            "Notification [{Type}] sent to user {UserId}: {Message}", type, userId, message);
    }

    /// <inheritdoc/>
    public async Task SendToAllAsync(
        NotificationType type, string message, CancellationToken ct = default)
    {
        await _hubContext.Clients.All.SendAsync(
            "ReceiveNotification",
            new { Type = type.ToString(), Message = message },
            ct);

        _logger.LogInformation("Broadcast notification [{Type}]: {Message}", type, message);
    }

    // ── Mapper ─────────────────────────────────────────────────────────────────
    private static NotificationDto MapToDto(Notification n)
        => new(n.Id, n.Type, n.Message, n.IsRead, n.CreatedAt);
}
