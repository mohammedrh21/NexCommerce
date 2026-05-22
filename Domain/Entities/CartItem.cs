namespace NexCommerce.Domain.Entities;

public class CartItem
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }

    // FK
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    /// <summary>
    /// References ProductVariant (not Product) so the specific size/color is captured.
    /// </summary>
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
}
