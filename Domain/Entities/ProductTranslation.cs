namespace NexCommerce.Domain.Entities;

public class ProductTranslation
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // FKs
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid LanguageId { get; set; }
    public Language Language { get; set; } = null!;
}
