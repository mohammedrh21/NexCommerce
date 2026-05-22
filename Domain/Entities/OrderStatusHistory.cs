using NexCommerce.Domain.Enums;

namespace NexCommerce.Domain.Entities;

public class OrderStatusHistory
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // FK
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
