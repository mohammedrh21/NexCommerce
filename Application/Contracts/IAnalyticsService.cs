using NexCommerce.Application.DTOs.Analytics;

namespace NexCommerce.Application.Contracts;

public interface IAnalyticsService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default);
    Task<VendorDashboardStatsDto> GetVendorDashboardAsync(Guid vendorId, CancellationToken ct = default);
    Task<VendorDashboardStatsDto> GetVendorDashboardByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<RevenueDataPointDto>> GetRevenueChartAsync(int months, CancellationToken ct = default);
    Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int count, CancellationToken ct = default);
    Task<IEnumerable<TopVendorDto>> GetTopVendorsAsync(int count, CancellationToken ct = default);
    Task<IEnumerable<UserGrowthDto>> GetUserGrowthAsync(int months, CancellationToken ct = default);
}
