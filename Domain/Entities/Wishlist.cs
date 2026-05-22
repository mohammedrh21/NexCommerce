namespace NexCommerce.Domain.Entities;

public class Wishlist
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FKs
    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
