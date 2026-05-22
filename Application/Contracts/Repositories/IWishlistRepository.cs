using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Contracts.Repositories;

public interface IWishlistRepository
{
    Task<(List<Wishlist> Items, int Total)> GetByUserPagedAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns the set of product IDs in the user's wishlist — lightweight; no entity graphs loaded.</summary>
    Task<IReadOnlySet<Guid>> GetProductIdsForUserAsync(Guid userId, CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task<Wishlist?> FindAsync(Guid userId, Guid productId, CancellationToken ct = default);
    void Add(Wishlist item);
    void Remove(Wishlist item);
}
