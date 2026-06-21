using Client.Models.Common;

namespace Client.Models.Orders;

public record PlaceOrderRequest(
    IEnumerable<PlaceOrderItemRequest> Items,
    string? CouponCode,
    string ShippingAddress);

public record PlaceOrderItemRequest(
    Guid ProductVariantId,
    int Quantity);

public record UpdateOrderStatusRequest(
    OrderStatus NewStatus,
    string? Note = null);

public record CancelOrderRequest(string Reason);

public record OrderStatusHistoryDto(
    OrderStatus Status,
    string? Note,
    DateTime ChangedAt);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ProductImage,
    Guid ProductVariantId,
    string? Size,
    string? Color,
    int Quantity,
    decimal Price,
    decimal LineTotal);

public record OrderDto(
    Guid Id,
    Guid UserId,
    string CustomerName,
    string CustomerEmail,
    decimal TotalAmount,
    decimal? DiscountAmount,
    OrderStatus Status,
    string ShippingAddress,
    DateTime CreatedAt,
    Guid? CouponId,
    string? CouponCode,
    IEnumerable<OrderItemDto> Items,
    IEnumerable<OrderStatusHistoryDto> StatusHistory,
    PaymentSummaryDto? Payment);

public record OrderSummaryDto(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    OrderStatus Status,
    int ItemCount,
    DateTime CreatedAt);

public record PaymentSummaryDto(
    Guid Id,
    decimal Amount,
    PaymentStatus Status,
    string? StripePaymentIntentId,
    DateTime? PaidAt);
