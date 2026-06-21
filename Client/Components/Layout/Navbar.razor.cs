using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Client.Models.Categories;
using Client.Services.Http;
using Client.Services;

namespace Client.Components.Layout;

public partial class Navbar : ComponentBase, IDisposable
{
    [Inject] private ICategoriesApiService CategoriesApi { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private ICartApiService CartApi { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private bool _isAuthenticated;
    private string _userName = string.Empty;
    private string _userEmail = string.Empty;
    private string _userRole = string.Empty;
    private int _cartItemCount;
    private List<CategoryListItemDto> _categories = new();
    private string _searchQuery = string.Empty;
    
    private bool _showCategoriesMenu;
    private bool _showUserMenu;
    private bool _showMobileMenu;

    protected override async Task OnInitializedAsync()
    {
        AuthStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        await CheckAuthAndLoadData();
        await LoadCategories();
    }

    private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        await InvokeAsync(async () =>
        {
            await CheckAuthAndLoadData();
        });
    }

    private async Task CheckAuthAndLoadData()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        _isAuthenticated = user.Identity?.IsAuthenticated ?? false;

        if (_isAuthenticated)
        {
            _userName = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value 
                        ?? user.FindFirst("unique_name")?.Value 
                        ?? user.FindFirst("name")?.Value 
                        ?? "User";
            _userEmail = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                         ?? user.FindFirst("email")?.Value 
                         ?? string.Empty;
            _userRole = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value 
                        ?? user.FindFirst("role")?.Value 
                        ?? string.Empty;

            await LoadCartCount();
        }
        else
        {
            _userName = string.Empty;
            _userEmail = string.Empty;
            _userRole = string.Empty;
            _cartItemCount = 0;
        }
        StateHasChanged();
    }

    private async Task LoadCategories()
    {
        try
        {
            var categories = await CategoriesApi.GetRootsAsync();
            if (categories != null)
            {
                _categories = categories.ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading categories: {ex.Message}");
        }
    }

    private async Task LoadCartCount()
    {
        try
        {
            var cart = await CartApi.GetCartAsync();
            _cartItemCount = cart?.TotalItems ?? 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading cart count: {ex.Message}");
        }
    }

    private void ToggleCategoriesMenu()
    {
        _showCategoriesMenu = !_showCategoriesMenu;
        _showUserMenu = false;
    }

    private void ToggleUserMenu()
    {
        _showUserMenu = !_showUserMenu;
        _showCategoriesMenu = false;
    }

    private void ToggleMobileMenu()
    {
        _showMobileMenu = !_showMobileMenu;
    }

    private void HandleSearch()
    {
        if (!string.IsNullOrWhiteSpace(_searchQuery))
        {
            Navigation.NavigateTo($"/search?q={Uri.EscapeDataString(_searchQuery)}");
        }
    }

    private void HandleSearchKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            HandleSearch();
        }
    }

    private void NavigateToCategory(Guid categoryId)
    {
        _showCategoriesMenu = false;
        _showMobileMenu = false;
        Navigation.NavigateTo($"/categories/{categoryId}");
    }

    private void CloseMenus()
    {
        _showCategoriesMenu = false;
        _showUserMenu = false;
    }

    public void Dispose()
    {
        AuthStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
