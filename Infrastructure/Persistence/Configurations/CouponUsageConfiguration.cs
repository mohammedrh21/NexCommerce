using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Configurations;

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.HasKey(x => x.Id);
        // One use per user per coupon
        builder.HasIndex(x => new { x.CouponId, x.UserId }).IsUnique();

        builder.HasOne(x => x.Coupon)
            .WithMany(c => c.Usages)
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<NexCommerce.Infrastructure.Identity.ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
