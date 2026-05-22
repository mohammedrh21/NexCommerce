using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(NexCommerceDbContext db) : IProductRepository
{
    private IQueryable<Product> BaseQuery => db.Products
        .Include(p => p.Translations).ThenInclude(t => t.Language)
        .Include(p => p.Images)
        .Include(p => p.Reviews)
        .Include(p => p.Vendor).ThenInclude(v => v.Translations).ThenInclude(t => t.Language)
        .Include(p => p.Category).ThenInclude(c => c.Translations).ThenInclude(t => t.Language);

    public async Task<(List<Product> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = BaseQuery.OrderByDescending(p => p.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(List<Product> Items, int Total)> GetByVendorPagedAsync(Guid vendorId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = BaseQuery.Where(p => p.VendorId == vendorId).OrderByDescending(p => p.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<int> CountByVendorAsync(Guid vendorId, CancellationToken ct = default)
        => await db.Products.CountAsync(p => p.VendorId == vendorId, ct);

    public async Task<int> CountLowStockProductsByVendorAsync(Guid vendorId, CancellationToken ct = default)
    {
        return await db.ProductVariants
            .Where(v => v.Product.VendorId == vendorId && v.StockQuantity <= v.LowStockThreshold)
            .Select(v => v.ProductId)
            .Distinct()
            .CountAsync(ct);
    }

    public async Task<(List<Product> Items, int Total)> GetByCategoryPagedAsync(Guid categoryId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = BaseQuery.Where(p => p.CategoryId == categoryId).OrderByDescending(p => p.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Product?> FindWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await BaseQuery
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Product?> FindWithVendorAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Products
            .Include(p => p.Vendor)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<ProductVariant>> GetVariantsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await db.ProductVariants
            .Include(v => v.Product).ThenInclude(p => p.Translations).ThenInclude(t => t.Language)
            .Include(v => v.Product).ThenInclude(p => p.Images)
            .Include(v => v.Product).ThenInclude(p => p.Vendor)
            .Where(v => ids.Contains(v.Id))
            .ToListAsync(ct);
    }

    public async Task<bool> VendorIsApprovedAsync(Guid vendorId, CancellationToken ct = default)
    {
        var vendor = await db.VendorProfiles.FirstOrDefaultAsync(v => v.Id == vendorId, ct);
        return vendor?.IsApproved ?? false;
    }

    public async Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await db.Categories.AnyAsync(c => c.Id == categoryId, ct);
    }

    public async Task<bool> IsVendorOwnerAsync(Guid productId, Guid vendorUserId, CancellationToken ct = default)
    {
        return await db.Products
            .Include(p => p.Vendor)
            .AnyAsync(p => p.Id == productId && p.Vendor.UserId == vendorUserId, ct);
    }

    public void Add(Product product) => db.Products.Add(product);
    public void Remove(Product product) => db.Products.Remove(product);

    public async Task RecordViewAsync(Guid productId, Guid userId, CancellationToken ct = default)
    {
        var alreadyViewed = await db.ProductViews.AnyAsync(v => v.ProductId == productId && v.UserId == userId, ct);
        if (!alreadyViewed)
            db.ProductViews.Add(new ProductView { ProductId = productId, UserId = userId, ViewedAt = DateTime.UtcNow });
    }

    public async Task<ProductImage?> FindImageAsync(Guid imageId, Guid productId, CancellationToken ct = default)
    {
        return await db.ProductImages
            .Include(i => i.Product).ThenInclude(p => p.Vendor)
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId, ct);
    }

    public void AddImage(ProductImage image) => db.ProductImages.Add(image);
    public void RemoveImage(ProductImage image) => db.ProductImages.Remove(image);

    public async Task<(List<Product> Items, int Total)> SearchAsync(
        string? keyword,
        Guid? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        decimal? minRating,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = BaseQuery;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p => p.Translations.Any(t => 
                EF.Functions.Like(t.Name, $"%{keyword}%") || 
                EF.Functions.Like(t.Description, $"%{keyword}%")));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (minRating.HasValue)
        {
            query = query.Where(p => p.Rating >= minRating.Value);
        }

        query = sortBy?.ToLower() switch
        {
            "price" => sortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "rating" => sortDescending ? query.OrderByDescending(p => p.Rating) : query.OrderBy(p => p.Rating),
            "name" => sortDescending 
                ? query.OrderByDescending(p => p.Translations.Select(t => t.Name).FirstOrDefault()) 
                : query.OrderBy(p => p.Translations.Select(t => t.Name).FirstOrDefault()),
            _ => sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
