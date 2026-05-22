namespace NexCommerce.Domain.Entities;

public class VendorProfile
{
    public Guid Id { get; set; }

    /// <summary>Set to true by Admin after vetting the vendor registration.</summary>
    public bool IsApproved { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FK (unique — one vendor per user)
    public Guid UserId { get; set; }

    // Navigation
    public ICollection<VendorProfileTranslation> Translations { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
