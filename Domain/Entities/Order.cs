using NexCommerce.Domain.Enums;

namespace NexCommerce.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FKs
    public Guid UserId { get; set; }

    public Guid? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    // Navigation
    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = [];
    public Payment? Payment { get; set; }
}
