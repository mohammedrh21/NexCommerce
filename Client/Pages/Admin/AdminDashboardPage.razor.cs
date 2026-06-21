using Client.Models.Analytics;
using Client.Services;
using Client.Models.Vendors;
using Client.Services.Http;
using Microsoft.AspNetCore.Components;

namespace Client.Pages.Admin;

public partial class AdminDashboardPage : ComponentBase
{
    [Inject] private IAnalyticsApiService AnalyticsApi { get; set; } = default!;
    [Inject] private IVendorsApiService VendorsApi { get; set; } = default!;
    [Inject] private IToastService ToastService { get; set; } = default!;

    private AdminDashboardDto? _dashboard;
    private IEnumerable<VendorListItemDto>? _pendingVendors;
    private bool _isLoading = true;
    private bool _isPendingLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(LoadDashboardAsync(), LoadPendingVendorsAsync());
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            _isLoading = true;
            _dashboard = await AnalyticsApi.GetAdminDashboardAsync();
        }
        catch (Exception ex)
        {
            ToastService.Error($"Failed to load dashboard: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadPendingVendorsAsync()
    {
        try
        {
            _isPendingLoading = true;
            var all = await VendorsApi.GetAllAsync(page: 1, pageSize: 50);
            _pendingVendors = all.Data.Where(v => !v.IsApproved).ToList();
        }
        catch (Exception)
        {
            _pendingVendors = [];
        }
        finally
        {
            _isPendingLoading = false;
        }
    }

    private async Task ApproveVendorAsync(Guid vendorId, bool approve)
    {
        try
        {
            await VendorsApi.ApproveVendorAsync(vendorId, approve);
            ToastService.Success(approve ? "Vendor approved." : "Vendor rejected.");
            await LoadPendingVendorsAsync();
        }
        catch (Exception ex)
        {
            ToastService.Error($"Action failed: {ex.Message}");
        }
    }
}
