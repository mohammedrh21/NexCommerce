using Client.Models.Common;
using Client.Models.Products;
using Client.Services;
using Client.Services.Http;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Pages.Wishlist;

public partial class WishlistPage : ComponentBase
{
    [Inject] private IWishlistsApiService WishlistsApi { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private PagedResult<ProductListItemDto>? _result;
    private List<ProductListItemDto> _wishlistItems = [];
    private bool _isLoading = true;
    private int _currentPage = 1;
    private const int PageSize = 12;

    protected override async Task OnInitializedAsync()
    {
        await LoadWishlistAsync();
    }

    private async Task LoadWishlistAsync()
    {
        try
        {
            _isLoading = true;
            _result = await WishlistsApi.GetWishlistAsync(_currentPage, PageSize);
            _wishlistItems = _result?.Data.ToList() ?? [];
        }
        catch (Exception ex)
        {
            _result = null;
            _wishlistItems = [];
            ToastService.Error($"Failed to load wishlist: {ex.Message}");
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
        await LoadWishlistAsync();
    }

    private async Task HandleRemoveFromWishlist(ProductListItemDto product)
    {
        try
        {
            // Instantly remove locally from the UI list for immediate feedback
            _wishlistItems.Remove(product);
            StateHasChanged();

            await WishlistsApi.RemoveFromWishlistAsync(product.Id);
            ToastService.Success($"{product.Name} removed from wishlist.");
            
            // Reload in background to ensure correct totals and pagination sync
            _result = await WishlistsApi.GetWishlistAsync(_currentPage, PageSize);
            _wishlistItems = _result?.Data.ToList() ?? [];
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ToastService.Error($"Failed to remove item: {ex.Message}");
            // Re-fetch in case of failure to restore consistency
            await LoadWishlistAsync();
        }
    }

    private void HandleAddToCart(ProductListItemDto product)
    {
        NavigationManager.NavigateTo($"/products/{product.Id}");
    }
}
