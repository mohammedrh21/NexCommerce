using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Analytics;

namespace NexCommerce.API.Controllers;

public class AnalyticsController(IAnalyticsService analyticsService) : ApiControllerBase
{
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminDashboard(CancellationToken ct = default)
    {
        var result = await analyticsService.GetAdminDashboardAsync(ct);
        return Ok(ApiResponse<AdminDashboardDto>.Ok(result));
    }

    [HttpGet("vendor")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> GetVendorDashboard(CancellationToken ct = default)
    {
        var result = await analyticsService.GetVendorDashboardByUserIdAsync(CurrentUserId, ct);
        return Ok(ApiResponse<VendorDashboardStatsDto>.Ok(result));
    }

    [HttpGet("revenue-chart")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRevenueChart([FromQuery] int months = 6, CancellationToken ct = default)
    {
        var result = await analyticsService.GetRevenueChartAsync(months, ct);
        return Ok(ApiResponse<IEnumerable<RevenueDataPointDto>>.Ok(result));
    }

    [HttpGet("top-products")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int count = 5, CancellationToken ct = default)
    {
        var result = await analyticsService.GetTopProductsAsync(count, ct);
        return Ok(ApiResponse<IEnumerable<TopProductDto>>.Ok(result));
    }

    [HttpGet("top-vendors")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTopVendors([FromQuery] int count = 5, CancellationToken ct = default)
    {
        var result = await analyticsService.GetTopVendorsAsync(count, ct);
        return Ok(ApiResponse<IEnumerable<TopVendorDto>>.Ok(result));
    }

    [HttpGet("user-growth")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserGrowth([FromQuery] int months = 6, CancellationToken ct = default)
    {
        var result = await analyticsService.GetUserGrowthAsync(months, ct);
        return Ok(ApiResponse<IEnumerable<UserGrowthDto>>.Ok(result));
    }
}
