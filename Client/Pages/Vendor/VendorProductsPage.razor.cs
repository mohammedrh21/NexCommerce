using Client.Models.Common;
using Client.Services;
using Client.Models.Products;
using Client.Models.Vendors;
using Client.Services.Http;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Vendor;

public partial class VendorProductsPage : ComponentBase
{
    [Inject] private IProductsApiService ProductsApi { get; set; } = default!;
    [Inject] private IVendorsApiService VendorsApi { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private PagedResult<ProductListItemDto>? _result;
    private VendorProfileDto? _profile;
    private bool _isLoading = true;
    private bool _isDeleting = false;
    private int _currentPage = 1;
    private const int PageSize = 15;

    private ProductListItemDto? _deleteTarget;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _profile = await VendorsApi.GetMyProfileAsync();
        }
        catch (Exception)
        {
            // Profile not found
        }

        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        if (_profile is null) { _isLoading = false; return; }

        try
        {
            _isLoading = true;
            _result = await ProductsApi.GetByVendorAsync(_profile.Id, _currentPage, PageSize);
        }
        catch (Exception ex)
        {
            ToastService.Error($"Failed to load products: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ConfirmDelete(ProductListItemDto product)
        => _deleteTarget = product;

    private async Task DeleteProductAsync()
    {
        if (_deleteTarget is null) return;

        try
        {
            _isDeleting = true;
            await ProductsApi.DeleteAsync(_deleteTarget.Id);
            ToastService.Success($"\"{_deleteTarget.Name}\" deleted.");
            _deleteTarget = null;
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            ToastService.Error($"Delete failed: {ex.Message}");
        }
        finally
        {
            _isDeleting = false;
        }
    }

    private async Task ChangePage(int page)
    {
        if (page < 1 || (_result is not null && page > _result.TotalPages)) return;
        _currentPage = page;
        await LoadProductsAsync();
    }
}
