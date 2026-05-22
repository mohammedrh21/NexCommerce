using NexCommerce.Domain.Enums;

namespace NexCommerce.Domain.Entities;

public class Coupon
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int UsageLimit { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CouponTranslation> Translations { get; set; } = [];
    public ICollection<CouponUsage> Usages { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
}
