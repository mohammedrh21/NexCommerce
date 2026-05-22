namespace NexCommerce.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }

    /// <summary>Price snapshot captured at order creation time.</summary>
    public decimal Price { get; set; }

    // FKs
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    /// <summary>Specific variant (size/color) that was ordered.</summary>
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    /// <summary>Direct product snapshot — avoids 4-table join for order history display.</summary>
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    /// <summary>Vendor snapshot — allows filtering orders per vendor without joining Order.</summary>
    public Guid VendorId { get; set; }
    public VendorProfile Vendor { get; set; } = null!;
}
