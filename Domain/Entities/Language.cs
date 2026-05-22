namespace NexCommerce.Domain.Entities;

public class Language
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;   // "en", "ar"
    public string Name { get; set; } = string.Empty;   // "English", "Arabic"
    public bool IsDefault { get; set; }
}
