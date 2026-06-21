using Client.Models.Common;
using Client.Models.Orders;
using Client.Services.Http;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Orders;

public partial class MyOrdersPage : ComponentBase
{
    [Inject] private IOrdersApiService OrdersApi { get; set; } = default!;

    private PagedResult<OrderSummaryDto>? _result;
    private bool _isLoading = true;
    private int _currentPage = 1;
    private const int PageSize = 10;

    protected override async Task OnInitializedAsync()
    {
        await LoadOrdersAsync();
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            _isLoading = true;
            _result = await OrdersApi.GetMyOrdersAsync(_currentPage, PageSize);
        }
        catch (Exception)
        {
            _result = null;
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
        await LoadOrdersAsync();
    }
}
