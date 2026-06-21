using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Client.Models.Notifications;
using Client.Services.Http;
using Client.Services;

namespace Client.Components.Layout;

public partial class NotificationBell : ComponentBase, IDisposable
{
    [Inject] private INotificationsApiService NotificationsApi { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private bool _isOpen;
    private bool _isAuthenticated;
    private List<NotificationDto> _notifications = new();
    private int _unreadCount;
    private System.Threading.Timer? _pollingTimer;

    protected override async Task OnInitializedAsync()
    {
        AuthStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        await CheckAuthAndFetchNotifications();

        // Simple polling for notification updates every 30 seconds
        _pollingTimer = new System.Threading.Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                if (_isAuthenticated)
                {
                    await FetchNotifications();
                }
            });
        }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        await InvokeAsync(async () =>
        {
            await CheckAuthAndFetchNotifications();
        });
    }

    private async Task CheckAuthAndFetchNotifications()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        _isAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;

        if (_isAuthenticated)
        {
            await FetchNotifications();
        }
        else
        {
            _notifications.Clear();
            _unreadCount = 0;
            StateHasChanged();
        }
    }

    private async Task FetchNotifications()
    {
        try
        {
            var response = await NotificationsApi.GetNotificationsAsync();
            if (response != null && response.Data != null)
            {
                _notifications = response.Data.OrderByDescending(n => n.CreatedAt).Take(5).ToList();
            }

            _unreadCount = await NotificationsApi.GetUnreadCountAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            // Fail silently for notification loading
            Console.WriteLine($"Error fetching notifications: {ex.Message}");
        }
    }

    private void ToggleDropdown()
    {
        _isOpen = !_isOpen;
    }

    private void CloseDropdown()
    {
        _isOpen = false;
    }

    private async Task MarkAllAsRead()
    {
        try
        {
            await NotificationsApi.MarkAllAsReadAsync();
            _unreadCount = 0;
            for (int i = 0; i < _notifications.Count; i++)
            {
                _notifications[i] = _notifications[i] with { IsRead = true };
            }
            ToastService.Success("All notifications marked as read.");
            StateHasChanged();
        }
        catch (Exception)
        {
            ToastService.Error("Failed to mark notifications as read.");
        }
    }

    private async Task HandleNotificationClick(NotificationDto notification)
    {
        CloseDropdown();
        
        if (!notification.IsRead)
        {
            try
            {
                await NotificationsApi.MarkAsReadAsync(new[] { notification.Id });
                _unreadCount = Math.Max(0, _unreadCount - 1);
                // Update local list
                var idx = _notifications.FindIndex(n => n.Id == notification.Id);
                if (idx >= 0)
                {
                    _notifications[idx] = _notifications[idx] with { IsRead = true };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking notification as read: {ex.Message}");
            }
        }

        // Navigate based on type
        // NotificationType.OrderUpdate = 1
        if (notification.Type == Models.Common.NotificationType.OrderUpdate)
        {
            Navigation.NavigateTo("/orders");
        }
        else if (notification.Type == Models.Common.NotificationType.Chat)
        {
            Navigation.NavigateTo("/vendor/chat"); // or customer chat if applicable
        }
        else
        {
            Navigation.NavigateTo("/notifications");
        }
    }

    private void ViewAll()
    {
        CloseDropdown();
        Navigation.NavigateTo("/notifications");
    }

    public void Dispose()
    {
        AuthStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        _pollingTimer?.Dispose();
    }
}
