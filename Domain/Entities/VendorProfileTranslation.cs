namespace NexCommerce.Domain.Entities;

public class VendorProfileTranslation
{
    public Guid Id { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // FKs
    public Guid VendorProfileId { get; set; }
    public VendorProfile VendorProfile { get; set; } = null!;

    public Guid LanguageId { get; set; }
    public Language Language { get; set; } = null!;
}
