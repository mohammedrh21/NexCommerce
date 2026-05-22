namespace NexCommerce.Domain.Entities;

public class CouponUsage
{
    public Guid Id { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    public Guid CouponId { get; set; }
    public Coupon Coupon { get; set; } = null!;

    public Guid UserId { get; set; }
}
