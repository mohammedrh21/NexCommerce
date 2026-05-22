using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.StripePaymentIntentId).HasMaxLength(200);

        builder.HasIndex(x => x.OrderId).IsUnique(); // one payment per order

        builder.HasOne(x => x.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<Payment>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
