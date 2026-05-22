using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Configurations;

public class VendorProfileTranslationConfiguration : IEntityTypeConfiguration<VendorProfileTranslation>
{
    public void Configure(EntityTypeBuilder<VendorProfileTranslation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StoreName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);

        builder.HasIndex(x => new { x.VendorProfileId, x.LanguageId }).IsUnique();

        builder.HasOne(x => x.Language)
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
