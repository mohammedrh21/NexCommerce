namespace NexCommerce.Application.DTOs.Search;

public record SearchRequest(
    string? Keyword = null,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    decimal? MinRating = null,
    string? SortBy = "createdAt",    // createdAt | price | rating | name
    bool SortDescending = true,
    int Page = 1,
    int PageSize = 20);

public record SearchResultItemDto(
    Guid ProductId,
    string Name,
    string? Description,
    decimal Price,
    decimal Rating,
    int ReviewCount,
    string? PrimaryImageUrl,
    string CategoryName,
    string VendorName,
    bool IsInWishlist);

public record SearchResultDto(
    IEnumerable<SearchResultItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
