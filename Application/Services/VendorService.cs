using Microsoft.Extensions.Logging;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Vendors;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Services;

public sealed class VendorService(
    IVendorRepository vendorRepository,
    IProductRepository productRepository,
    IIdentityService identityService,
    ILogger<VendorService> logger)
    : IVendorService
{
    public async Task<PagedResult<VendorListItemDto>> GetAllAsync(int page, int pageSize, string lang, CancellationToken ct = default)
    {
        var (vendors, total) = await vendorRepository.GetPagedAsync(page, pageSize, ct);

        var result = new List<VendorListItemDto>();
        foreach (var v in vendors)
        {
            var user = await identityService.GetUserInfoAsync(v.UserId);
            result.Add(MapListItem(v, user, lang));
        }

        return new PagedResult<VendorListItemDto> { Data = result, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<VendorProfileDto> GetByIdAsync(Guid vendorId, string lang, CancellationToken ct = default)
    {
        var vendor = await vendorRepository.FindByIdAsync(vendorId, ct)
            ?? throw new NotFoundException($"Vendor {vendorId} not found.");

        var user = await identityService.GetUserInfoAsync(vendor.UserId);
        return MapDto(vendor, user, lang);
    }

    public async Task<VendorProfileDto> GetByUserIdAsync(Guid userId, string lang, CancellationToken ct = default)
    {
        var vendor = await vendorRepository.FindByUserIdAsync(userId, ct)
            ?? throw new NotFoundException($"Vendor profile not found for user {userId}.");

        var user = await identityService.GetUserInfoAsync(vendor.UserId);
        return MapDto(vendor, user, lang);
    }



    public async Task<VendorStatsDto> GetStatsAsync(Guid vendorId, CancellationToken ct = default)
    {
        logger.LogInformation("Retrieving statistics for vendor {VendorId}", vendorId);

        var totalProducts = await productRepository.CountByVendorAsync(vendorId, ct);
        var lowStockProducts = await productRepository.CountLowStockProductsByVendorAsync(vendorId, ct);
        
        return new VendorStatsDto(
            TotalProducts:    totalProducts,
            TotalOrders:      0,
            TotalRevenue:     0,
            PendingOrders:    0,
            LowStockProducts: lowStockProducts);
    }

    private static VendorProfileDto MapDto(VendorProfile v, UserInfoDto user, string lang) => new(
        Id:           v.Id,
        UserId:       v.UserId,
        OwnerName:    user.FullName,
        OwnerEmail:   user.Email,
        IsApproved:   v.IsApproved,
        CreatedAt:    v.CreatedAt,
        ProductCount: v.Products.Count,
        Translations: v.Translations.Select(t => new VendorTranslationDto(t.Language.Code, t.StoreName, t.Description)));

    private static VendorListItemDto MapListItem(VendorProfile v, UserInfoDto user, string lang) => new(
        Id:           v.Id,
        UserId:       v.UserId,
        StoreName:    TranslationHelper.TranslateVendor(v.Translations, lang),
        OwnerName:    user.FullName,
        IsApproved:   v.IsApproved,
        ProductCount: v.Products.Count,
        CreatedAt:    v.CreatedAt);
}

