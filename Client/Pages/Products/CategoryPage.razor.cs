using Client.Models.Categories;
using Client.Models.Common;
using Client.Models.Products;
using Client.Services.Http;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Products;

public partial class CategoryPage : ComponentBase
{
    [Inject] private ICategoriesApiService CategoriesApiService { get; set; } = default!;
    [Inject] private IProductsApiService ProductsApiService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public Guid Id { get; set; }

    private CategoryDto? _category;
    private CategoryTranslationDto? _translation;
    private PagedResult<ProductListItemDto>? _productsResult;

    private bool _isLoading = true;
    private bool _isLoadingProducts = false;
    private string? _errorMessage;

    private int _currentPage = 1;
    private const int PageSize = 12;

    protected override async Task OnParametersSetAsync()
    {
        // Reset state when category ID changes
        _currentPage = 1;
        await LoadCategoryAndProductsAsync();
    }

    private async Task LoadCategoryAndProductsAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;

            _category = await CategoriesApiService.GetByIdAsync(Id);
            
            if (_category != null)
            {
                _translation = _category.Translations?.FirstOrDefault(t => t.LanguageCode == "en") 
                               ?? _category.Translations?.FirstOrDefault();
            }

            await LoadProductsAsync();
        }
        catch (Exception)
        {
            _errorMessage = "Failed to load category details. Please try again later.";
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
            _productsResult = await ProductsApiService.GetByCategoryAsync(Id, _currentPage, PageSize);
        }
        catch (Exception)
        {
            // Optional: show toast or set error state for products specifically
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
