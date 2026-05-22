namespace NexCommerce.Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public int Rating { get; set; }      // 1 – 5
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FKs
    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
