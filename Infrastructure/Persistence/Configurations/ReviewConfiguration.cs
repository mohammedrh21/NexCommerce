using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
        // Product relationship defined in ProductConfiguration

        builder.HasOne<NexCommerce.Infrastructure.Identity.ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
