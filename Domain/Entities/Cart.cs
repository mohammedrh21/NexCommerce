namespace NexCommerce.Domain.Entities;

public class Cart
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // FK
    public Guid UserId { get; set; }

    // Navigation
    public ICollection<CartItem> Items { get; set; } = [];
}
