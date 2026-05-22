using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.ShippingAddress).HasMaxLength(500);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);

        builder.HasOne<NexCommerce.Infrastructure.Identity.ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Coupon)
            .WithMany(c => c.Orders)
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Items owned by OrderItemConfiguration
        // StatusHistory owned by OrderStatusHistoryConfiguration

        builder.HasOne(x => x.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

