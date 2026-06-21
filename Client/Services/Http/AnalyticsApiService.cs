using Client.Models.Analytics;

namespace Client.Services.Http;

public interface IAnalyticsApiService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default);
    Task<VendorDashboardStatsDto> GetVendorDashboardAsync(CancellationToken ct = default);
    Task<IEnumerable<RevenueDataPointDto>> GetRevenueChartAsync(int months = 6, CancellationToken ct = default);
    Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int count = 5, CancellationToken ct = default);
    Task<IEnumerable<TopVendorDto>> GetTopVendorsAsync(int count = 5, CancellationToken ct = default);
    Task<IEnumerable<UserGrowthDto>> GetUserGrowthAsync(int months = 6, CancellationToken ct = default);
}

public class AnalyticsApiService : BaseApiService, IAnalyticsApiService
{
    public AnalyticsApiService(HttpClient httpClient) : base(httpClient) { }

    public Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default)
        => GetAsync<AdminDashboardDto>("api/v1/analytics/admin", ct);

    public Task<VendorDashboardStatsDto> GetVendorDashboardAsync(CancellationToken ct = default)
        => GetAsync<VendorDashboardStatsDto>("api/v1/analytics/vendor", ct);

    public Task<IEnumerable<RevenueDataPointDto>> GetRevenueChartAsync(int months = 6, CancellationToken ct = default)
        => GetAsync<IEnumerable<RevenueDataPointDto>>($"api/v1/analytics/revenue-chart?months={months}", ct);

    public Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int count = 5, CancellationToken ct = default)
        => GetAsync<IEnumerable<TopProductDto>>($"api/v1/analytics/top-products?count={count}", ct);

    public Task<IEnumerable<TopVendorDto>> GetTopVendorsAsync(int count = 5, CancellationToken ct = default)
        => GetAsync<IEnumerable<TopVendorDto>>($"api/v1/analytics/top-vendors?count={count}", ct);

    public Task<IEnumerable<UserGrowthDto>> GetUserGrowthAsync(int months = 6, CancellationToken ct = default)
        => GetAsync<IEnumerable<UserGrowthDto>>($"api/v1/analytics/user-growth?months={months}", ct);
}
