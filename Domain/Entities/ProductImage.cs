namespace NexCommerce.Domain.Entities;

public class ProductImage
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Cloudinary public ID used to delete the image later.</summary>
    public string PublicId { get; set; } = string.Empty;

    public bool IsMain { get; set; }

    // FK
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
