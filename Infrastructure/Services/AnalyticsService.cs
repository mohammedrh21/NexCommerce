using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Analytics;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Enums;
using NexCommerce.Infrastructure.Persistence;
using System.Globalization;

namespace NexCommerce.Infrastructure.Services;

public sealed class AnalyticsService(NexCommerceDbContext db) : IAnalyticsService
{
    public async Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalUsers = await db.Users.CountAsync(ct);
        var totalVendors = await db.VendorProfiles.CountAsync(ct);
        var totalProducts = await db.Products.CountAsync(ct);
        var totalOrders = await db.Orders.CountAsync(ct);
        
        var totalRevenue = await db.Orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0m;

        var pendingOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.Pending, ct);
        
        var newUsersThisMonth = await db.Users
            .CountAsync(u => u.CreatedAt >= firstDayOfMonth, ct);
            
        var revenueThisMonth = await db.Orders
            .Where(o => o.Status != OrderStatus.Cancelled && o.CreatedAt >= firstDayOfMonth)
            .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0m;

        var stats = new DashboardStatsDto(
            TotalUsers: totalUsers,
            TotalVendors: totalVendors,
            TotalProducts: totalProducts,
            TotalOrders: totalOrders,
            TotalRevenue: totalRevenue,
            PendingOrders: pendingOrders,
            NewUsersThisMonth: newUsersThisMonth,
            RevenueThisMonth: revenueThisMonth);

        var revenueChart = await GetRevenueChartAsync(6, ct);
        var topProducts = await GetTopProductsAsync(5, ct);
        var topVendors = await GetTopVendorsAsync(5, ct);
        var userGrowth = await GetUserGrowthAsync(6, ct);

        return new AdminDashboardDto(stats, revenueChart, topProducts, topVendors, userGrowth);
    }

    public async Task<VendorDashboardStatsDto> GetVendorDashboardAsync(Guid vendorId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalProducts = await db.Products.CountAsync(p => p.VendorId == vendorId, ct);
        
        // Active products have at least one variant with stock
        var activeProducts = await db.Products
            .CountAsync(p => p.VendorId == vendorId && p.Variants.Any(v => v.StockQuantity > 0), ct);

        var lowStockProducts = await db.ProductVariants
            .Where(v => v.Product.VendorId == vendorId && v.StockQuantity <= v.LowStockThreshold)
            .Select(v => v.ProductId)
            .Distinct()
            .CountAsync(ct);

        var totalOrders = await db.OrderItems
            .Where(oi => oi.VendorId == vendorId)
            .Select(oi => oi.OrderId)
            .Distinct()
            .CountAsync(ct);

        var pendingOrders = await db.OrderItems
            .Where(oi => oi.VendorId == vendorId && oi.Order.Status == OrderStatus.Pending)
            .Select(oi => oi.OrderId)
            .Distinct()
            .CountAsync(ct);

        var totalRevenue = await db.OrderItems
            .Where(oi => oi.VendorId == vendorId && oi.Order.Status != OrderStatus.Cancelled)
            .SumAsync(oi => (decimal?)(oi.Price * oi.Quantity), ct) ?? 0m;

        var revenueThisMonth = await db.OrderItems
            .Where(oi => oi.VendorId == vendorId && oi.Order.Status != OrderStatus.Cancelled && oi.Order.CreatedAt >= firstDayOfMonth)
            .SumAsync(oi => (decimal?)(oi.Price * oi.Quantity), ct) ?? 0m;

        // Vendor specific chart (past 6 months)
        var startDate = now.AddMonths(-6);
        var orderItems = await db.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.VendorId == vendorId && oi.Order.Status != OrderStatus.Cancelled && oi.Order.CreatedAt >= startDate)
            .ToListAsync(ct);

        var revenueChart = orderItems
            .GroupBy(oi => new { oi.Order.CreatedAt.Year, oi.Order.CreatedAt.Month })
            .Select(g => new RevenueDataPointDto(
                Label: new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Revenue: g.Sum(oi => oi.Price * oi.Quantity),
                OrderCount: g.Select(oi => oi.OrderId).Distinct().Count()
            ))
            .OrderBy(dp => DateTime.ParseExact(dp.Label, "MMM yyyy", CultureInfo.InvariantCulture))
            .ToList();

        // Vendor specific top products
        var topProductAggs = orderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new {
                ProductId = g.Key,
                TotalSold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.Price * oi.Quantity)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(5)
            .ToList();

        var topProductIds = topProductAggs.Select(x => x.ProductId).ToList();
        var products = await db.Products
            .Include(p => p.Translations).ThenInclude(t => t.Language)
            .Include(p => p.Images)
            .Where(p => topProductIds.Contains(p.Id))
            .ToListAsync(ct);

        var topProducts = topProductAggs.Select(tp => {
            var p = products.FirstOrDefault(x => x.Id == tp.ProductId);
            var name = p?.Translations.FirstOrDefault(t => t.Language.Code == "en")?.Name 
                    ?? p?.Translations.FirstOrDefault()?.Name ?? "Product";
            var img = p?.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? p?.Images.FirstOrDefault()?.ImageUrl;
            return new TopProductDto(tp.ProductId, name, img, tp.TotalSold, tp.Revenue);
        }).ToList();

        return new VendorDashboardStatsDto(
            TotalProducts: totalProducts,
            ActiveProducts: activeProducts,
            LowStockProducts: lowStockProducts,
            TotalOrders: totalOrders,
            PendingOrders: pendingOrders,
            TotalRevenue: totalRevenue,
            RevenueThisMonth: revenueThisMonth,
            RevenueChart: revenueChart,
            TopProducts: topProducts);
    }

    public async Task<VendorDashboardStatsDto> GetVendorDashboardByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var vendor = await db.VendorProfiles.FirstOrDefaultAsync(v => v.UserId == userId, ct)
            ?? throw new NotFoundException($"Vendor profile not found for user {userId}.");

        return await GetVendorDashboardAsync(vendor.Id, ct);
    }

    public async Task<IEnumerable<RevenueDataPointDto>> GetRevenueChartAsync(int months, CancellationToken ct = default)
    {
        var startDate = DateTime.UtcNow.AddMonths(-months);
        var orders = await db.Orders
            .Where(o => o.Status != OrderStatus.Cancelled && o.CreatedAt >= startDate)
            .ToListAsync(ct);

        return orders
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new RevenueDataPointDto(
                Label: new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Revenue: g.Sum(o => o.TotalAmount),
                OrderCount: g.Count()
            ))
            .OrderBy(dp => DateTime.ParseExact(dp.Label, "MMM yyyy", CultureInfo.InvariantCulture))
            .ToList();
    }

    public async Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int count, CancellationToken ct = default)
    {
        var topProductAggs = await db.OrderItems
            .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new {
                ProductId = g.Key,
                TotalSold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => (decimal?)(oi.Price * oi.Quantity)) ?? 0m
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(count)
            .ToListAsync(ct);

        var topProductIds = topProductAggs.Select(x => x.ProductId).ToList();
        var products = await db.Products
            .Include(p => p.Translations).ThenInclude(t => t.Language)
            .Include(p => p.Images)
            .Where(p => topProductIds.Contains(p.Id))
            .ToListAsync(ct);

        return topProductAggs.Select(tp => {
            var p = products.FirstOrDefault(x => x.Id == tp.ProductId);
            var name = p?.Translations.FirstOrDefault(t => t.Language.Code == "en")?.Name 
                    ?? p?.Translations.FirstOrDefault()?.Name ?? "Product";
            var img = p?.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? p?.Images.FirstOrDefault()?.ImageUrl;
            return new TopProductDto(tp.ProductId, name, img, tp.TotalSold, tp.Revenue);
        }).ToList();
    }

    public async Task<IEnumerable<TopVendorDto>> GetTopVendorsAsync(int count, CancellationToken ct = default)
    {
        var topVendorAggs = await db.OrderItems
            .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
            .GroupBy(oi => oi.VendorId)
            .Select(g => new {
                VendorId = g.Key,
                TotalOrders = g.Select(oi => oi.OrderId).Distinct().Count(),
                Revenue = g.Sum(oi => (decimal?)(oi.Price * oi.Quantity)) ?? 0m
            })
            .OrderByDescending(x => x.Revenue)
            .Take(count)
            .ToListAsync(ct);

        var topVendorIds = topVendorAggs.Select(x => x.VendorId).ToList();
        var vendors = await db.VendorProfiles
            .Include(v => v.Translations).ThenInclude(t => t.Language)
            .Where(v => topVendorIds.Contains(v.Id))
            .ToListAsync(ct);

        var dtos = new List<TopVendorDto>();
        foreach (var tv in topVendorAggs)
        {
            var v = vendors.FirstOrDefault(x => x.Id == tv.VendorId);
            var storeName = v?.Translations.FirstOrDefault(t => t.Language.Code == "en")?.StoreName 
                         ?? v?.Translations.FirstOrDefault()?.StoreName ?? "Store";

            var ownerName = "Vendor Owner";
            if (v is not null)
            {
                var u = await db.Users.FirstOrDefaultAsync(user => user.Id == v.UserId, ct);
                ownerName = u?.FullName ?? "Vendor Owner";
            }

            dtos.Add(new TopVendorDto(tv.VendorId, storeName, ownerName, tv.TotalOrders, tv.Revenue));
        }

        return dtos;
    }

    public async Task<IEnumerable<UserGrowthDto>> GetUserGrowthAsync(int months, CancellationToken ct = default)
    {
        var startDate = DateTime.UtcNow.AddMonths(-months);
        var users = await db.Users.Where(u => u.CreatedAt >= startDate).ToListAsync(ct);
        var vendors = await db.VendorProfiles.Where(v => v.CreatedAt >= startDate).ToListAsync(ct);

        var allMonths = Enumerable.Range(0, months)
            .Select(i => DateTime.UtcNow.AddMonths(-i))
            .OrderBy(d => d.Year).ThenBy(d => d.Month)
            .ToList();

        return allMonths.Select(m => {
            var label = m.ToString("MMM yyyy", CultureInfo.InvariantCulture);
            var newUsers = users.Count(u => u.CreatedAt.Year == m.Year && u.CreatedAt.Month == m.Month);
            var newVendors = vendors.Count(v => v.CreatedAt.Year == m.Year && v.CreatedAt.Month == m.Month);
            return new UserGrowthDto(label, newUsers, newVendors);
        }).ToList();
    }
}
