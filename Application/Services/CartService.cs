using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Cart;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Services;

public sealed class CartService(
    ICartRepository cartRepository,
    ILogger<CartService> logger)
    : ICartService
{
    public async Task<CartDto> GetCartAsync(Guid userId, CancellationToken ct = default)
    {
        var cart = await cartRepository.FindByUserIdAsync(userId, ct);

        if (cart is null)
        {
            logger.LogInformation("No cart found for user {UserId}, returning empty cart", userId);
            return new CartDto(Guid.Empty, userId, [], 0m, 0, DateTime.UtcNow);
        }

        return MapDto(cart);
    }

    // ── Mapping ──────────────────────────────────────────────────────────────

    private static CartDto MapDto(Cart cart)
    {
        var items = cart.Items.Select(i =>
        {
            var variant   = i.ProductVariant;
            var product   = variant?.Product;
            var name      = product?.Translations.FirstOrDefault()?.Name ?? string.Empty;
            var image     = product?.Images.FirstOrDefault(img => img.IsMain)?.ImageUrl
                         ?? product?.Images.FirstOrDefault()?.ImageUrl;
            var unitPrice = (product?.Price ?? 0) + (variant?.PriceAdjustment ?? 0);

            return new CartItemDto(
                Id:               i.Id,
                ProductVariantId: i.ProductVariantId,
                ProductId:        variant?.ProductId ?? Guid.Empty,
                ProductName:      name,
                ProductImage:     image,
                Size:             variant?.Size,
                Color:            variant?.Color,
                SKU:              variant?.SKU,
                Quantity:         i.Quantity,
                UnitPrice:        unitPrice,
                LineTotal:        unitPrice * i.Quantity,
                AvailableStock:   variant?.StockQuantity ?? 0);
        }).ToList();

        return new CartDto(
            Id:         cart.Id,
            UserId:     cart.UserId,
            Items:      items,
            SubTotal:   items.Sum(i => i.LineTotal),
            TotalItems: items.Sum(i => i.Quantity),
            UpdatedAt:  cart.UpdatedAt);
    }
}
