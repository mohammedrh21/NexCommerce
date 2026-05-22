using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Contracts.Repositories;

public interface ICartRepository
{
    /// <summary>Returns cart with items, each item loaded with its ProductVariant + Product + translations + images.</summary>
    Task<Cart?> FindByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<CartItem?> FindItemAsync(Guid cartItemId, Guid userId, CancellationToken ct = default);

    void AddCart(Cart cart);
    void AddItem(CartItem item);
    void RemoveItem(CartItem item);
}
