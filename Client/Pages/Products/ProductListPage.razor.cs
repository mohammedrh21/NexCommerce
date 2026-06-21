using Client.Models.Categories;
using Client.Models.Products;
using Client.Models.Search;
using Client.Services;
using Client.Services.Http;
using Client.State.Auth;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Pages.Products;

public partial class ProductListPage : FluxorComponent
{
    [Inject] private IState<AuthState> AuthState { get; set; } = default!;
    [Inject] private ISearchApiService SearchApi { get; set; } = default!;
    [Inject] private ICategoriesApiService CategoriesApi { get; set; } = default!;
    [Inject] private IWishlistsApiService WishlistsApi { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter][SupplyParameterFromQuery(Name = "q")] public string? Keyword { get; set; }
    [Parameter][SupplyParameterFromQuery(Name = "categoryId")] public Guid? CategoryId { get; set; }
    [Parameter][SupplyParameterFromQuery(Name = "minPrice")] public decimal? MinPrice { get; set; }
    [Parameter][SupplyParameterFromQuery(Name = "maxPrice")] public decimal? MaxPrice { get; set; }
    [Parameter][SupplyParameterFromQuery(Name = "minRating")] public decimal? MinRating { get; set; }
    [Parameter][SupplyParameterFromQuery(Name = "sortBy")] public string? SortBy { get; set; } = "createdAt";
    [Parameter][SupplyParameterFromQuery(Name = "sortDescending")] public bool SortDescending { get; set; } = true;
    [Parameter][SupplyParameterFromQuery(Name = "page")] public int Page { get; set; } = 1;

    private SearchResultDto? _searchResult;
    private IEnumerable<CategoryListItemDto> _categories = [];
    private HashSet<Guid> _wishlistProductIds = [];
    
    private bool _isLoadingProducts = true;

    // Temporary local model for price inputs to prevent premature searches while typing
    private decimal? _localMinPrice;
    private decimal? _localMaxPrice;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        _localMinPrice = MinPrice;
        _localMaxPrice = MaxPrice;

        // Load categories once
        await LoadCategories();

        if (AuthState.Value.IsAuthenticated)
        {
            await LoadWishlist();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        
        // Ensure local filters match URL parameters on parameter set (e.g., URL updated)
        _localMinPrice = MinPrice;
        _localMaxPrice = MaxPrice;
        
        await LoadProducts();
    }

    private async Task LoadCategories()
    {
        try
        {
            _categories = await CategoriesApi.GetAllAsync();
        }
        catch (Exception ex)
        {
            ToastService.Error($"Failed to load categories: {ex.Message}");
        }
    }

    private async Task LoadWishlist()
    {
        try
        {
            var result = await WishlistsApi.GetWishlistAsync(page: 1, pageSize: 100);
            _wishlistProductIds = result.Data.Select(p => p.Id).ToHashSet();
        }
        catch
        {
            // Fail silently
        }
    }

    private async Task LoadProducts()
    {
        try
        {
            _isLoadingProducts = true;

            var request = new SearchRequest(
                Keyword: Keyword,
                CategoryId: CategoryId,
                MinPrice: MinPrice,
                MaxPrice: MaxPrice,
                MinRating: MinRating,
                SortBy: SortBy ?? "createdAt",
                SortDescending: SortDescending,
                Page: Page,
                PageSize: 12
            );

            _searchResult = await SearchApi.SearchAsync(request);
        }
        catch (Exception ex)
        {
            ToastService.Error($"Failed to load products: {ex.Message}");
        }
        finally
        {
            _isLoadingProducts = false;
        }
    }

    private void ApplyFilters()
    {
        var parameters = new Dictionary<string, object?>
        {
            ["minPrice"] = _localMinPrice,
            ["maxPrice"] = _localMaxPrice,
            ["page"] = 1 // reset page when filters change
        };

        UpdateUrlParameters(parameters);
    }

    private void ClearFilters()
    {
        _localMinPrice = null;
        _localMaxPrice = null;

        var parameters = new Dictionary<string, object?>
        {
            ["q"] = null,
            ["categoryId"] = null,
            ["minPrice"] = null,
            ["maxPrice"] = null,
            ["minRating"] = null,
            ["sortBy"] = "createdAt",
            ["sortDescending"] = true,
            ["page"] = 1
        };

        UpdateUrlParameters(parameters);
    }

    private void SetCategory(Guid? categoryId)
    {
        UpdateUrlParameters(new Dictionary<string, object?>
        {
            ["categoryId"] = categoryId,
            ["page"] = 1
        });
    }

    private void SetRating(decimal? rating)
    {
        UpdateUrlParameters(new Dictionary<string, object?>
        {
            ["minRating"] = rating,
            ["page"] = 1
        });
    }

    private void SetSort(string sortField, bool descending)
    {
        UpdateUrlParameters(new Dictionary<string, object?>
        {
            ["sortBy"] = sortField,
            ["sortDescending"] = descending,
            ["page"] = 1
        });
    }

    private void ChangePage(int newPage)
    {
        UpdateUrlParameters(new Dictionary<string, object?>
        {
            ["page"] = newPage
        });
    }

    private void UpdateUrlParameters(Dictionary<string, object?> newParameters)
    {
        var nextUri = NavigationManager.GetUriWithQueryParameters(newParameters);
        NavigationManager.NavigateTo(nextUri);
    }

    private void HandleAddToCart(ProductListItemDto product)
    {
        NavigationManager.NavigateTo($"/products/{product.Id}");
    }

    private async Task HandleWishlistToggle(ProductListItemDto product)
    {
        if (!AuthState.Value.IsAuthenticated)
        {
            ToastService.Warning("Please log in to add items to your wishlist.", "Authentication Required");
            NavigationManager.NavigateTo($"login?returnUrl={Uri.EscapeDataString(NavigationManager.ToBaseRelativePath(NavigationManager.Uri))}");
            return;
        }

        try
        {
            if (_wishlistProductIds.Contains(product.Id))
            {
                await WishlistsApi.RemoveFromWishlistAsync(product.Id);
                _wishlistProductIds.Remove(product.Id);
                ToastService.Success($"{product.Name} removed from wishlist.");
            }
            else
            {
                await WishlistsApi.AddToWishlistAsync(product.Id);
                _wishlistProductIds.Add(product.Id);
                ToastService.Success($"{product.Name} added to wishlist.");
            }
        }
        catch (Exception ex)
        {
            ToastService.Error($"Wishlist operation failed: {ex.Message}");
        }
    }

    private ProductListItemDto MapSearchResultToListItem(SearchResultItemDto r)
    {
        return new ProductListItemDto(
            r.ProductId,
            r.Name,
            r.Description,
            r.Price,
            r.Rating,
            r.ReviewCount,
            r.PrimaryImageUrl,
            Guid.Empty,
            r.CategoryName,
            Guid.Empty,
            r.VendorName,
            DateTime.UtcNow
        );
    }
}
