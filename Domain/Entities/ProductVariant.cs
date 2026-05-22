namespace NexCommerce.Domain.Entities;

public class ProductVariant
{
    public Guid Id { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? SKU { get; set; }
    public int StockQuantity { get; set; }

    /// <summary>Stock level below which the variant is flagged as low-stock. Default 5.</summary>
    public int LowStockThreshold { get; set; } = 5;

    /// <summary>
    /// Optional price delta from the base Product.Price. null = same price.
    /// </summary>
    public decimal? PriceAdjustment { get; set; }

    // FK
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Navigation
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
