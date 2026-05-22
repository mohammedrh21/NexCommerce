using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Rating).HasPrecision(3, 2);

        builder.HasIndex(x => x.VendorId);
        builder.HasIndex(x => x.CategoryId);

        // Configured here to avoid double-configuration with CategoryConfiguration
        builder.HasOne(x => x.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // All child collection relationships are owned by their respective child configs:
        // ProductTranslation → ProductTranslationConfiguration
        // ProductVariant     → ProductVariantConfiguration
        // ProductImage       → ProductImageConfiguration
        // Review             → ReviewConfiguration
        // ProductView        → ProductViewConfiguration
        // Vendor             → VendorProfileConfiguration
    }
}

