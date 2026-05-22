namespace NexCommerce.Domain.Entities;

public class CouponTranslation
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;

    public Guid CouponId { get; set; }
    public Coupon Coupon { get; set; } = null!;

    public Guid LanguageId { get; set; }
    public Language Language { get; set; } = null!;
}
