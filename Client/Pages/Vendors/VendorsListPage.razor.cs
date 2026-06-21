using Client.Models.Common;
using Client.Models.Vendors;
using Client.Services.Http;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Vendors;

public partial class VendorsListPage : ComponentBase
{
    [Inject] private IVendorsApiService VendorsApiService { get; set; } = default!;

    private PagedResult<VendorListItemDto>? _vendorsResult;
    private bool _isLoading = true;
    private string? _errorMessage;

    private int _currentPage = 1;
    private const int PageSize = 12;

    protected override async Task OnInitializedAsync()
    {
        await LoadVendorsAsync();
    }

    private async Task LoadVendorsAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;

            _vendorsResult = await VendorsApiService.GetAllAsync(_currentPage, PageSize);
        }
        catch (Exception)
        {
            _errorMessage = "Failed to load vendors. Please try again later.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task OnPageChanged(int page)
    {
        _currentPage = page;
        await LoadVendorsAsync();
    }
}
