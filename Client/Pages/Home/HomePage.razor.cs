using Client.Models.Categories;
using Client.Models.Products;
using Client.Models.Recommendations;
using Client.Models.Vendors;
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

namespace Client.Pages.Home;

public partial class HomePage : FluxorComponent
{
    [Inject] private IState<AuthState> AuthState { get; set; } = default!;
    [Inject] private IRecommendationsApiService RecommendationsApi { get; set; } = default!;
    [Inject] private ICategoriesApiService CategoriesApi { get; set; } = default!;
    [Inject] private IVendorsApiService VendorsApi { get; set; } = default!;
    [Inject] private IWishlistsApiService WishlistsApi { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private IEnumerable<RecommendationDto> _trendingProducts = [];
    private IEnumerable<RecommendationDto> _personalizedProducts = [];
    private IEnumerable<CategoryListItemDto> _categories = [];
    private IEnumerable<VendorListItemDto> _vendors = [];
    
    private HashSet<Guid> _wishlistProductIds = [];

    private bool _isLoadingTrending = true;
    private bool _isLoadingPersonalized = true;
    private bool _isLoadingCategories = true;
    private bool _isLoadingVendors = true;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Start loading basic contents in parallel
        var trendingTask = LoadTrendingProducts();
        var categoriesTask = LoadCategories();
        var vendorsTask = LoadVendors();

        await Task.WhenAll(trendingTask, categoriesTask, vendorsTask);

        if (AuthState.Value.IsAuthenticated)
        {
            await LoadPersonalizedProducts();
            await LoadWishlist();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        
        // If auth state changes, load personalized recommendations and wishlist
        if (AuthState.Value.IsAuthenticated && !_personalizedProducts.Any() && !_isLoadingPersonalized)
        {
            await LoadPersonalizedProducts();
            await LoadWishlist();
        }
        else if (!AuthState.Value.IsAuthenticated)
        {
            _personalizedProducts = [];
            _wishlistProductIds.Clear();
        }
    }

    private async Task LoadTrendingProducts()
    {
        try
        {
            _isLoadingTrending = true;
            _trendingProducts = await RecommendationsApi.GetTrendingAsync(8);
        }
        catch (Exception ex)
        {
            ToastService.Error($"Failed to load trending products: {ex.Message}");
        }
        finally
        {
            _isLoadingTrending = false;
        }
    }

    private async Task LoadPersonalizedProducts()
    {
        try
        {
            _isLoadingPersonalized = true;
            _personalizedProducts = await RecommendationsApi.GetPersonalizedAsync(8);
        }
        catch (Exception)
        {
            // Fail silently - personalized recommendations might fail for new/unauthenticated users
            _personalizedProducts = [];
        }
        finally
        {
            _isLoadingPersonalized = false;
        }
    }

    private async Task LoadCategories()
    {
        try
        {
            _isLoadingCategories = true;
            _categories = await CategoriesApi.GetRootsAsync();
        }
        catch (Exception ex)
        {
            ToastService.Error($"Failed to load categories: {ex.Message}");
        }
        finally
        {
            _isLoadingCategories = false;
        }
    }

    private async Task LoadVendors()
    {
        try
        {
            _isLoadingVendors = true;
            var result = await VendorsApi.GetAllAsync(page: 1, pageSize: 6);
            _vendors = result.Data;
        }
        catch (Exception ex)
        {
            ToastService.Error($"Failed to load vendors: {ex.Message}");
        }
        finally
        {
            _isLoadingVendors = false;
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

    private void HandleAddToCart(ProductListItemDto product)
    {
        // Redirect to detail page so user can choose variants/options
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

    private ProductListItemDto MapToProductListItem(RecommendationDto r)
    {
        return new ProductListItemDto(
            r.ProductId,
            r.Name,
            r.Description,
            r.Price,
            r.Rating,
            0,
            r.PrimaryImageUrl,
            Guid.Empty,
            r.CategoryName,
            Guid.Empty,
            r.VendorName,
            DateTime.UtcNow
        );
    }
}
