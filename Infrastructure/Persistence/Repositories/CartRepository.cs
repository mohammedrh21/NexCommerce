using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Repositories;

public sealed class CartRepository(NexCommerceDbContext db) : ICartRepository
{
    public async Task<Cart?> FindByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product).ThenInclude(p => p.Translations).ThenInclude(t => t.Language)
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
    }

    public async Task<CartItem?> FindItemAsync(Guid cartItemId, Guid userId, CancellationToken ct = default)
    {
        return await db.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId, ct);
    }

    public void AddCart(Cart cart) => db.Carts.Add(cart);
    public void AddItem(CartItem item) => db.CartItems.Add(item);
    public void RemoveItem(CartItem item) => db.CartItems.Remove(item);
}
