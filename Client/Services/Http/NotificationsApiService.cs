using Client.Models.Common;
using Client.Models.Notifications;

namespace Client.Services.Http;

public interface INotificationsApiService
{
    Task<PagedResult<NotificationDto>> GetNotificationsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(CancellationToken ct = default);
    Task MarkAsReadAsync(IEnumerable<Guid> notificationIds, CancellationToken ct = default);
    Task MarkAllAsReadAsync(CancellationToken ct = default);
}

public class NotificationsApiService : BaseApiService, INotificationsApiService
{
    public NotificationsApiService(HttpClient httpClient) : base(httpClient) { }

    public Task<PagedResult<NotificationDto>> GetNotificationsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
        => GetAsync<PagedResult<NotificationDto>>($"api/v1/notifications?page={page}&pageSize={pageSize}", ct);

    public Task<int> GetUnreadCountAsync(CancellationToken ct = default)
        => GetAsync<int>("api/v1/notifications/unread-count", ct);

    public Task MarkAsReadAsync(IEnumerable<Guid> notificationIds, CancellationToken ct = default)
        => PostAsync<IEnumerable<Guid>, object>("api/v1/notifications/mark-read", notificationIds, ct);

    public Task MarkAllAsReadAsync(CancellationToken ct = default)
        => PostAsync<object, object>("api/v1/notifications/mark-all-read", new { }, ct);
}
