using Client.Models.Products;
using Client.Models.Vendors;
using Client.Services.Http;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Vendor;

public partial class VendorDashboardPage : ComponentBase
{
    [Inject] private IVendorsApiService VendorsApi { get; set; } = default!;
    [Inject] private IProductsApiService ProductsApi { get; set; } = default!;

    private VendorProfileDto? _profile;
    private VendorStatsDto? _stats;
    private IEnumerable<ProductListItemDto>? _recentProducts;
    private bool _isLoading = true;

    private string _storeName => _profile?.Translations
        .FirstOrDefault(t => t.LanguageCode == "en")?.StoreName
        ?? _profile?.Translations.FirstOrDefault()?.StoreName
        ?? "Your Store";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _isLoading = true;
            _profile = await VendorsApi.GetMyProfileAsync();

            if (_profile.IsApproved)
            {
                var statsTask = VendorsApi.GetStatsAsync(_profile.Id);
                var productsTask = ProductsApi.GetByVendorAsync(_profile.Id, page: 1, pageSize: 5);

                await Task.WhenAll(statsTask, productsTask);

                _stats = statsTask.Result;
                _recentProducts = productsTask.Result.Data;
            }
        }
        catch (Exception)
        {
            // Profile might not exist yet — handled gracefully in UI
        }
        finally
        {
            _isLoading = false;
        }
    }
}
