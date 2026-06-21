using Client.Models.Orders;
using Client.Services;
using Client.Services.Http;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Orders;

public partial class OrderDetailPage : ComponentBase
{
    [Inject] private IOrdersApiService OrdersApi { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public Guid Id { get; set; }

    private OrderDto? _order;
    private bool _isLoading = true;
    private string? _errorMessage;

    private bool _showCancelConfirm = false;
    private bool _isCancelling = false;
    private string? _cancelReason;

    protected override async Task OnInitializedAsync()
    {
        await LoadOrderAsync();
    }

    private async Task LoadOrderAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            _order = await OrdersApi.GetByIdAsync(Id);
        }
        catch (Exception ex)
        {
            _errorMessage = $"Could not load order: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task CancelOrderAsync()
    {
        try
        {
            _isCancelling = true;
            await OrdersApi.CancelOrderAsync(Id, new CancelOrderRequest(_cancelReason ?? "Cancelled by customer"));
            _showCancelConfirm = false;
            ToastService.Success("Order cancelled successfully.");
            await LoadOrderAsync();
        }
        catch (Exception ex)
        {
            ToastService.Error($"Failed to cancel order: {ex.Message}");
        }
        finally
        {
            _isCancelling = false;
        }
    }
}
