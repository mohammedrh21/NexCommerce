using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Products;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Services;

public sealed class WishlistService(
    IWishlistRepository wishlistRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IWishlistService
{
    public async Task<PagedResult<ProductListItemDto>> GetWishlistAsync(Guid userId, int page, int pageSize, string lang, CancellationToken ct = default)
    {
        var (items, total) = await wishlistRepository.GetByUserPagedAsync(userId, page, pageSize, ct);
        var productList = items.Select(w => MapListItem(w.Product, lang)).ToList();

        return new PagedResult<ProductListItemDto>
        {
            Data = productList,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task AddToWishlistAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        var product = await productRepository.FindWithVendorAsync(productId, ct)
            ?? throw new NotFoundException($"Product {productId} not found.");

        var exists = await wishlistRepository.ExistsAsync(userId, productId, ct);
        if (exists) return;

        var item = new Wishlist
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        };

        wishlistRepository.Add(item);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RemoveFromWishlistAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        var item = await wishlistRepository.FindAsync(userId, productId, ct);
        if (item is not null)
        {
            wishlistRepository.Remove(item);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> IsInWishlistAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        return await wishlistRepository.ExistsAsync(userId, productId, ct);
    }

    // ── Translation & Mapping Helpers ──────────────────────────────────────────

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
}
