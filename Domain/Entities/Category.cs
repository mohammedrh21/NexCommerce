namespace NexCommerce.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }

    // Self-referencing for subcategories
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> SubCategories { get; set; } = [];

    // Navigation
    public ICollection<CategoryTranslation> Translations { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
