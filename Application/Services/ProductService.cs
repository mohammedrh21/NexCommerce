using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Products;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Services;

public sealed class ProductService(IProductRepository productRepository) : IProductService
{
    public async Task<PagedResult<ProductListItemDto>> GetAllAsync(int page, int pageSize, string lang, CancellationToken ct = default)
    {
        var (items, total) = await productRepository.GetPagedAsync(page, pageSize, ct);
        return new PagedResult<ProductListItemDto> { Data = items.Select(p => MapListItem(p, lang)), TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<ProductListItemDto>> GetByVendorAsync(Guid vendorId, int page, int pageSize, string lang, CancellationToken ct = default)
    {
        var (items, total) = await productRepository.GetByVendorPagedAsync(vendorId, page, pageSize, ct);
        return new PagedResult<ProductListItemDto> { Data = items.Select(p => MapListItem(p, lang)), TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<ProductListItemDto>> GetByCategoryAsync(Guid categoryId, int page, int pageSize, string lang, CancellationToken ct = default)
    {
        var (items, total) = await productRepository.GetByCategoryPagedAsync(categoryId, page, pageSize, ct);
        return new PagedResult<ProductListItemDto> { Data = items.Select(p => MapListItem(p, lang)), TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<ProductDetailDto> GetByIdAsync(Guid id, Guid? requestingUserId, string lang, CancellationToken ct = default)
    {
        var product = await productRepository.FindWithDetailsAsync(id, ct)
            ?? throw new NotFoundException($"Product {id} not found.");

        if (requestingUserId.HasValue)
            await RecordViewAsync(id, requestingUserId.Value, ct);

        return MapDetail(product, lang);
    }


    public async Task RecordViewAsync(Guid productId, Guid userId, CancellationToken ct = default)
    {
        await productRepository.RecordViewAsync(productId, userId, ct);
    }

    // ── Mapping helpers ───────────────────────────────────────────────────────

    private static ProductListItemDto MapListItem(Product p, string lang) => new(
        Id:             p.Id,
        Name:           TranslationHelper.Translate(p.Translations, lang, t => t.Name),
        Description:    TranslationHelper.Translate(p.Translations, lang, t => t.Description),
        Price:          p.Price,
        Rating:         p.Rating,
        ReviewCount:    p.Reviews.Count,
        PrimaryImageUrl: p.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl,
        CategoryId:     p.CategoryId,
        CategoryName:   TranslationHelper.TranslateCategory(p.Category.Translations, lang),
        VendorId:       p.VendorId,
        VendorName:     TranslationHelper.TranslateVendor(p.Vendor.Translations, lang),
        CreatedAt:      p.CreatedAt);

    private static ProductDetailDto MapDetail(Product p, string lang) => new(
        Id:           p.Id,
        Price:        p.Price,
        Rating:       p.Rating,
        ReviewCount:  p.Reviews.Count,
        CreatedAt:    p.CreatedAt,
        VendorId:     p.VendorId,
        VendorName:   TranslationHelper.TranslateVendor(p.Vendor.Translations, lang),
        CategoryId:   p.CategoryId,
        CategoryName: TranslationHelper.TranslateCategory(p.Category.Translations, lang),
        Translations: p.Translations.Select(t => new ProductTranslationDto(t.Language.Code, t.Name, t.Description)),
        Variants:     p.Variants.Select(v => new ProductVariantDto(v.Id, v.Size, v.Color, v.SKU,
                          v.StockQuantity, v.LowStockThreshold, v.PriceAdjustment, v.StockQuantity <= v.LowStockThreshold)),
        Images:       p.Images.Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.PublicId, i.IsMain)));
}

