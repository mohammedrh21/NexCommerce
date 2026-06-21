using Client.Models.Products;
using Client.Services.Http;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Products;

public partial class ProductDetailPage : ComponentBase
{
    [Inject] private IProductsApiService ProductsApiService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public Guid Id { get; set; }

    private ProductDetailDto? _product;
    private bool _isLoading = true;
    private string? _errorMessage;

    private ProductVariantDto? _selectedVariant;
    private ProductTranslationDto? _translation;

    protected override async Task OnInitializedAsync()
    {
        await LoadProductAsync();
    }

    private async Task LoadProductAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;

            _product = await ProductsApiService.GetByIdAsync(Id);

            if (_product != null)
            {
                // Fallback to first translation or English
                _translation = _product.Translations?.FirstOrDefault(t => t.LanguageCode == "en") 
                               ?? _product.Translations?.FirstOrDefault();

                // Select default variant if available
                _selectedVariant = _product.Variants?.FirstOrDefault();
            }
        }
        catch (Exception)
        {
            _errorMessage = "Failed to load product details. Please try again later.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void SelectVariant(ProductVariantDto variant)
    {
        _selectedVariant = variant;
    }

    private void AddToCart()
    {
        // TODO: Phase 7 - Cart Integration
        // ToastService.ShowSuccess("Added to cart");
    }
}
