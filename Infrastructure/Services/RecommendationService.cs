using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Recommendations;
using NexCommerce.Domain.Entities;
using NexCommerce.Infrastructure.Persistence;

namespace NexCommerce.Infrastructure.Services;

public sealed class RecommendationService(NexCommerceDbContext db) : IRecommendationService
{
    public async Task<IEnumerable<RecommendationDto>> GetSimilarProductsAsync(
        Guid productId, int count, string lang, CancellationToken ct = default)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null) return [];

        var similar = await db.Products
            .Include(p => p.Translations).ThenInclude(t => t.Language)
            .Include(p => p.Images)
            .Include(p => p.Vendor).ThenInclude(v => v.Translations).ThenInclude(t => t.Language)
            .Include(p => p.Category).ThenInclude(c => c.Translations).ThenInclude(t => t.Language)
            .Where(p => p.CategoryId == product.CategoryId && p.Id != productId)
            .OrderByDescending(p => p.Rating)
            .Take(count)
            .ToListAsync(ct);

        var reason = lang == "ar" ? "منتج مشابه" : "Similar Product";

        return similar.Select(p => new RecommendationDto(
            ProductId: p.Id,
            Name: Translate(p.Translations, lang, t => t.Name),
            Description: Translate(p.Translations, lang, t => t.Description),
            Price: p.Price,
            Rating: p.Rating,
            PrimaryImageUrl: p.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl,
            CategoryName: TranslateCat(p.Category.Translations, lang),
            VendorName: TranslateVendor(p.Vendor.Translations, lang),
            ReasonLabel: reason
        ));
    }

    public async Task<IEnumerable<RecommendationDto>> GetPersonalizedAsync(
        Guid userId, int count, string lang, CancellationToken ct = default)
    {
        var viewedProductIds = await db.ProductViews
            .Where(v => v.UserId == userId)
            .Select(v => v.ProductId)
            .ToListAsync(ct);

        var viewedCategoryIds = await db.Products
            .Where(p => viewedProductIds.Contains(p.Id))
            .Select(p => p.CategoryId)
            .Distinct()
            .ToListAsync(ct);

        var recommended = await db.Products
            .Include(p => p.Translations).ThenInclude(t => t.Language)
            .Include(p => p.Images)
            .Include(p => p.Vendor).ThenInclude(v => v.Translations).ThenInclude(t => t.Language)
            .Include(p => p.Category).ThenInclude(c => c.Translations).ThenInclude(t => t.Language)
            .Where(p => viewedCategoryIds.Contains(p.CategoryId) && !viewedProductIds.Contains(p.Id))
            .OrderByDescending(p => p.Rating)
            .Take(count)
            .ToListAsync(ct);

        if (recommended.Count < count)
        {
            var extraCount = count - recommended.Count;
            var trendingExtra = await db.Products
                .Include(p => p.Translations).ThenInclude(t => t.Language)
                .Include(p => p.Images)
                .Include(p => p.Vendor).ThenInclude(v => v.Translations).ThenInclude(t => t.Language)
                .Include(p => p.Category).ThenInclude(c => c.Translations).ThenInclude(t => t.Language)
                .Where(p => !viewedProductIds.Contains(p.Id) && !recommended.Select(rp => rp.Id).Contains(p.Id))
                .OrderByDescending(p => p.Rating)
                .Take(extraCount)
                .ToListAsync(ct);
            recommended.AddRange(trendingExtra);
        }

        var reason = lang == "ar" ? "موصى به لك" : "Recommended for You";

        return recommended.Select(p => new RecommendationDto(
            ProductId: p.Id,
            Name: Translate(p.Translations, lang, t => t.Name),
            Description: Translate(p.Translations, lang, t => t.Description),
            Price: p.Price,
            Rating: p.Rating,
            PrimaryImageUrl: p.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl,
            CategoryName: TranslateCat(p.Category.Translations, lang),
            VendorName: TranslateVendor(p.Vendor.Translations, lang),
            ReasonLabel: reason
        ));
    }

    public async Task<IEnumerable<RecommendationDto>> GetTrendingAsync(
        int count, string lang, CancellationToken ct = default)
    {
        var trendingIds = await db.ProductViews
            .GroupBy(v => v.ProductId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(count)
            .ToListAsync(ct);

        if (trendingIds.Count < count)
        {
            var extraIds = await db.Products
                .OrderByDescending(p => p.Rating)
                .Select(p => p.Id)
                .Take(count - trendingIds.Count)
                .ToListAsync(ct);
            trendingIds = trendingIds.Union(extraIds).Take(count).ToList();
        }

        var trending = await db.Products
            .Include(p => p.Translations).ThenInclude(t => t.Language)
            .Include(p => p.Images)
            .Include(p => p.Vendor).ThenInclude(v => v.Translations).ThenInclude(t => t.Language)
            .Include(p => p.Category).ThenInclude(c => c.Translations).ThenInclude(t => t.Language)
            .Where(p => trendingIds.Contains(p.Id))
            .ToListAsync(ct);

        // Maintain the order of trendingIds
        var orderedTrending = trendingIds
            .Select(id => trending.FirstOrDefault(p => p.Id == id))
            .Where(p => p is not null)
            .Cast<Product>()
            .ToList();

        var reason = lang == "ar" ? "شائع" : "Trending";

        return orderedTrending.Select(p => new RecommendationDto(
            ProductId: p.Id,
            Name: Translate(p.Translations, lang, t => t.Name),
            Description: Translate(p.Translations, lang, t => t.Description),
            Price: p.Price,
            Rating: p.Rating,
            PrimaryImageUrl: p.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl,
            CategoryName: TranslateCat(p.Category.Translations, lang),
            VendorName: TranslateVendor(p.Vendor.Translations, lang),
            ReasonLabel: reason
        ));
    }

    // ── Translation & Mapping Helpers ──────────────────────────────────────────

    private static string Translate(IEnumerable<ProductTranslation> translations, string lang, Func<ProductTranslation, string> selector)
    {
        var t = translations.FirstOrDefault(x => x.Language.Code == lang)
             ?? translations.FirstOrDefault(x => x.Language.Code == "en")
             ?? translations.FirstOrDefault();
        return t is not null ? selector(t) : string.Empty;
    }

    private static string TranslateCat(IEnumerable<CategoryTranslation> translations, string lang)
    {
        var t = translations.FirstOrDefault(x => x.Language.Code == lang)
             ?? translations.FirstOrDefault(x => x.Language.Code == "en")
             ?? translations.FirstOrDefault();
        return t?.Name ?? string.Empty;
    }

    private static string TranslateVendor(IEnumerable<VendorProfileTranslation> translations, string lang)
    {
        var t = translations.FirstOrDefault(x => x.Language.Code == lang)
             ?? translations.FirstOrDefault(x => x.Language.Code == "en")
             ?? translations.FirstOrDefault();
        return t?.StoreName ?? string.Empty;
    }
}
