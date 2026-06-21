using Client.Models.Common;
using Client.Services;
using Client.Models.Orders;
using Client.Services.Http;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Vendor;

public partial class VendorOrdersPage : ComponentBase
{
    [Inject] private IOrdersApiService OrdersApi { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private PagedResult<OrderSummaryDto>? _result;
    private bool _isLoading = true;
    private int _currentPage = 1;
    private const int PageSize = 15;

    protected override async Task OnInitializedAsync()
        => await LoadOrdersAsync();

    private async Task LoadOrdersAsync()
    {
        try
        {
            _isLoading = true;
            _result = await OrdersApi.GetVendorOrdersAsync(_currentPage, PageSize);
        }
        catch (Exception ex)
        {
            ToastService.Error($"Failed to load orders: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task MarkProcessingAsync(Guid orderId)
    {
        try
        {
            await OrdersApi.UpdateStatusAsync(orderId,
                new UpdateOrderStatusRequest(OrderStatus.Processing, "Order confirmed by vendor"));
            ToastService.Success("Order marked as Processing.");
            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            ToastService.Error($"Failed to update order: {ex.Message}");
        }
    }

    private async Task ChangePage(int page)
    {
        if (page < 1 || (_result is not null && page > _result.TotalPages)) return;
        _currentPage = page;
        await LoadOrdersAsync();
    }
}
