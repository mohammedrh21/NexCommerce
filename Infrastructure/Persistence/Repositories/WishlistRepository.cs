using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Repositories;

public sealed class WishlistRepository(NexCommerceDbContext db) : IWishlistRepository
{
    public async Task<(List<Wishlist> Items, int Total)> GetByUserPagedAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Wishlists
            .Include(w => w.Product).ThenInclude(p => p.Translations).ThenInclude(t => t.Language)
            .Include(w => w.Product).ThenInclude(p => p.Images)
            .Include(w => w.Product).ThenInclude(p => p.Vendor).ThenInclude(v => v.Translations).ThenInclude(t => t.Language)
            .Include(w => w.Product).ThenInclude(p => p.Category).ThenInclude(c => c.Translations).ThenInclude(t => t.Language)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        return await db.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductId == productId, ct);
    }

    public async Task<Wishlist?> FindAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        return await db.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, ct);
    }

    public void Add(Wishlist item) => db.Wishlists.Add(item);
    public void Remove(Wishlist item) => db.Wishlists.Remove(item);

    public async Task<IReadOnlySet<Guid>> GetProductIdsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var ids = await db.Wishlists
            .Where(w => w.UserId == userId)
            .Select(w => w.ProductId)
            .ToListAsync(ct);

        return ids.ToHashSet();
    }
}
