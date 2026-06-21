namespace Client.Models.Analytics;

public record DashboardStatsDto(
    int TotalUsers,
    int TotalVendors,
    int TotalProducts,
    int TotalOrders,
    decimal TotalRevenue,
    int PendingOrders,
    int NewUsersThisMonth,
    decimal RevenueThisMonth);

public record RevenueDataPointDto(
    string Label,
    decimal Revenue,
    int OrderCount);

public record TopProductDto(
    Guid ProductId,
    string ProductName,
    string? ImageUrl,
    int TotalSold,
    decimal Revenue);

public record TopVendorDto(
    Guid VendorId,
    string StoreName,
    string OwnerName,
    int TotalOrders,
    decimal Revenue);

public record UserGrowthDto(
    string Label,
    int NewUsers,
    int NewVendors);

public record VendorDashboardStatsDto(
    int TotalProducts,
    int ActiveProducts,
    int LowStockProducts,
    int TotalOrders,
    int PendingOrders,
    decimal TotalRevenue,
    decimal RevenueThisMonth,
    IEnumerable<RevenueDataPointDto> RevenueChart,
    IEnumerable<TopProductDto> TopProducts);

public record AdminDashboardDto(
    DashboardStatsDto Stats,
    IEnumerable<RevenueDataPointDto> RevenueChart,
    IEnumerable<TopProductDto> TopProducts,
    IEnumerable<TopVendorDto> TopVendors,
    IEnumerable<UserGrowthDto> UserGrowth);
