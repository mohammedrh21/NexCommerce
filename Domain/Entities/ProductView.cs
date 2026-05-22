namespace NexCommerce.Domain.Entities;

/// <summary>
/// Tracks which products a user has viewed — used for AI co-occurrence recommendations.
/// </summary>
public class ProductView
{
    public Guid Id { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    // FKs
    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
