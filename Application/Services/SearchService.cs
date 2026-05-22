using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Search;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Services;

public sealed class SearchService(
    IProductRepository productRepository,
    IWishlistRepository wishlistRepository) : ISearchService
{
    public async Task<SearchResultDto> SearchAsync(
        SearchRequest request, Guid? requestingUserId, string lang, CancellationToken ct = default)
    {
        var (items, total) = await productRepository.SearchAsync(
            request.Keyword,
            request.CategoryId,
            request.MinPrice,
            request.MaxPrice,
            request.MinRating,
            request.SortBy,
            request.SortDescending,
            request.Page,
            request.PageSize,
            ct);

        IReadOnlySet<Guid> wishlistProductIds = new HashSet<Guid>();
        if (requestingUserId.HasValue)
        {
            wishlistProductIds = await wishlistRepository.GetProductIdsForUserAsync(requestingUserId.Value, ct);
        }

        var searchItems = items.Select(p => new SearchResultItemDto(
            ProductId: p.Id,
            Name: TranslationHelper.Translate(p.Translations, lang, t => t.Name),
            Description: TranslationHelper.Translate(p.Translations, lang, t => t.Description),
            Price: p.Price,
            Rating: p.Rating,
            ReviewCount: p.Reviews.Count,
            PrimaryImageUrl: p.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl,
            CategoryName: TranslationHelper.TranslateCategory(p.Category.Translations, lang),
            VendorName: TranslationHelper.TranslateVendor(p.Vendor.Translations, lang),
            IsInWishlist: wishlistProductIds.Contains(p.Id)
        )).ToList();

        var totalPages = (int)Math.Ceiling((double)total / request.PageSize);

        return new SearchResultDto(
            Items: searchItems,
            TotalCount: total,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalPages: totalPages
        );
    }
}
