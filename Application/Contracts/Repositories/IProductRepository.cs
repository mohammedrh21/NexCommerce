using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Contracts.Repositories;

public interface IProductRepository
{
    Task<(List<Product> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<(List<Product> Items, int Total)> GetByVendorPagedAsync(Guid vendorId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns the total number of products for a vendor — no entity graphs loaded.</summary>
    Task<int> CountByVendorAsync(Guid vendorId, CancellationToken ct = default);
    Task<int> CountLowStockProductsByVendorAsync(Guid vendorId, CancellationToken ct = default);

    Task<(List<Product> Items, int Total)> GetByCategoryPagedAsync(Guid categoryId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns product with all navigation properties loaded (translations, variants, images, reviews, vendor, category).</summary>
    Task<Product?> FindWithDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lightweight load — only includes Vendor (for ownership check).</summary>
    Task<Product?> FindWithVendorAsync(Guid id, CancellationToken ct = default);

    Task<List<ProductVariant>> GetVariantsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<bool> VendorIsApprovedAsync(Guid vendorId, CancellationToken ct = default);
    Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>True if the product's vendor's UserId matches the given userId.</summary>
    Task<bool> IsVendorOwnerAsync(Guid productId, Guid vendorUserId, CancellationToken ct = default);

    void Add(Product product);
    void Remove(Product product);

    Task RecordViewAsync(Guid productId, Guid userId, CancellationToken ct = default);

    Task<ProductImage?> FindImageAsync(Guid imageId, Guid productId, CancellationToken ct = default);
    void AddImage(ProductImage image);
    void RemoveImage(ProductImage image);

    Task<(List<Product> Items, int Total)> SearchAsync(
        string? keyword,
        Guid? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        decimal? minRating,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
