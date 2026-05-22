using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.DiscountValue).HasPrecision(18, 2);

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.Coupon)
            .HasForeignKey(t => t.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Usages)
            .WithOne(u => u.Coupon)
            .HasForeignKey(u => u.CouponId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
