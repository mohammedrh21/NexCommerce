namespace NexCommerce.Application.DTOs.Cart;

// ── Requests ─────────────────────────────────────────────────────────────────

public record AddToCartRequest(
    Guid ProductVariantId,
    int Quantity);

public record UpdateCartItemRequest(int Quantity);

// ── Response DTOs ─────────────────────────────────────────────────────────────

public record CartItemDto(
    Guid Id,
    Guid ProductVariantId,
    Guid ProductId,
    string ProductName,
    string? ProductImage,
    string? Size,
    string? Color,
    string? SKU,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    int AvailableStock);

public record CartDto(
    Guid Id,
    Guid UserId,
    IEnumerable<CartItemDto> Items,
    decimal SubTotal,
    int TotalItems,
    DateTime UpdatedAt);
