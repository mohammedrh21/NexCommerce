namespace NexCommerce.Domain.Entities;

public class CategoryTranslation
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // FKs
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid LanguageId { get; set; }
    public Language Language { get; set; } = null!;
}
