using Client.Models.Common;
using Client.Models.Products;
using Client.Models.Vendors;
using Client.Services.Http;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Vendors;

public partial class VendorPublicPage : ComponentBase
{
    [Inject] private IVendorsApiService VendorsApiService { get; set; } = default!;
    [Inject] private IProductsApiService ProductsApiService { get; set; } = default!;

    [Parameter] public Guid Id { get; set; }

    private VendorProfileDto? _vendor;
    private VendorTranslationDto? _translation;
    private PagedResult<ProductListItemDto>? _productsResult;

    private bool _isLoading = true;
    private bool _isLoadingProducts = false;
    private string? _errorMessage;

    private int _currentPage = 1;
    private const int PageSize = 12;

    protected override async Task OnParametersSetAsync()
    {
        _currentPage = 1;
        await LoadVendorAndProductsAsync();
    }

    private async Task LoadVendorAndProductsAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;

            _vendor = await VendorsApiService.GetByIdAsync(Id);
            
            if (_vendor != null)
            {
                _translation = _vendor.Translations?.FirstOrDefault(t => t.LanguageCode == "en") 
                               ?? _vendor.Translations?.FirstOrDefault();
            }

            await LoadProductsAsync();
        }
        catch (Exception)
        {
            _errorMessage = "Failed to load vendor profile. Please try again later.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            _isLoadingProducts = true;
            _productsResult = await ProductsApiService.GetByVendorAsync(Id, _currentPage, PageSize);
        }
        catch (Exception)
        {
            // Handled gracefully without breaking the entire page
        }
        finally
        {
            _isLoadingProducts = false;
        }
    }

    private async Task OnPageChanged(int page)
    {
        _currentPage = page;
        await LoadProductsAsync();
    }
}
