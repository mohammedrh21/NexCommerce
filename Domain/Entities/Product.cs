namespace NexCommerce.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }

    /// <summary>
    /// Denormalized average rating — recomputed whenever a new review is saved.
    /// </summary>
    public decimal Rating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FKs
    public Guid VendorId { get; set; }
    public VendorProfile Vendor { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Navigation
    public ICollection<ProductTranslation> Translations { get; set; } = [];
    public ICollection<ProductVariant> Variants { get; set; } = [];
    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<ProductView> Views { get; set; } = [];
}
