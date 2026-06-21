using Client.Models.Common;
using Client.Models.Notifications;
using Client.Services;
using Client.Services.Http;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Pages.Notifications;

public partial class NotificationsPage : ComponentBase
{
    [Inject] private INotificationsApiService NotificationsApi { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private PagedResult<NotificationDto>? _result;
    private List<NotificationDto> _notifications = [];
    private int _unreadCount = 0;
    private bool _isLoading = true;
    private bool _isMarkingAll = false;
    private int _currentPage = 1;
    private const int PageSize = 10;

    protected override async Task OnInitializedAsync()
    {
        await LoadNotificationsAsync();
    }

    private async Task LoadNotificationsAsync()
    {
        try
        {
            _isLoading = true;
            _result = await NotificationsApi.GetNotificationsAsync(_currentPage, PageSize);
            _notifications = _result?.Data.ToList() ?? [];
            _unreadCount = await NotificationsApi.GetUnreadCountAsync();
        }
        catch (Exception ex)
        {
            _result = null;
            _notifications = [];
            _unreadCount = 0;
            ToastService.Error($"Failed to load notifications: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task ChangePage(int page)
    {
        if (page < 1 || (_result is not null && page > _result.TotalPages)) return;
        _currentPage = page;
        await LoadNotificationsAsync();
    }

    private async Task MarkAsReadAsync(NotificationDto notification)
    {
        try
        {
            // Optimistic UI update
            var index = _notifications.IndexOf(notification);
            if (index != -1)
            {
                _notifications[index] = notification with { IsRead = true };
                if (_unreadCount > 0) _unreadCount--;
                StateHasChanged();
            }

            await NotificationsApi.MarkAsReadAsync([notification.Id]);
        }
        catch (Exception ex)
        {
            ToastService.Error($"Failed to mark notification as read: {ex.Message}");
            // Reload on failure to restore status
            await LoadNotificationsAsync();
        }
    }

    private async Task MarkAllAsReadAsync()
    {
        try
        {
            _isMarkingAll = true;

            // Optimistic UI update
            for (int i = 0; i < _notifications.Count; i++)
            {
                _notifications[i] = _notifications[i] with { IsRead = true };
            }
            _unreadCount = 0;
            StateHasChanged();

            await NotificationsApi.MarkAllAsReadAsync();
            ToastService.Success("All notifications marked as read.");
        }
        catch (Exception ex)
        {
            ToastService.Error($"Failed to mark all as read: {ex.Message}");
            await LoadNotificationsAsync();
        }
        finally
        {
            _isMarkingAll = false;
        }
    }
}
