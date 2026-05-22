using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Size).HasMaxLength(50);
        builder.Property(x => x.Color).HasMaxLength(50);
        builder.Property(x => x.SKU).HasMaxLength(100);
        builder.Property(x => x.PriceAdjustment).HasPrecision(18, 2);
        builder.Property(x => x.LowStockThreshold).HasDefaultValue(5);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.SKU).IsUnique();

        builder.HasOne(x => x.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
