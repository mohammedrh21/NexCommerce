using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(NexCommerceDbContext db) : ICategoryRepository
{
    private IQueryable<Category> BaseQuery => db.Categories
        .Include(c => c.Translations).ThenInclude(t => t.Language)
        .Include(c => c.SubCategories)
        .Include(c => c.Products);

    public async Task<List<Category>> GetAllAsync(CancellationToken ct = default)
    {
        return await BaseQuery
            .Include(c => c.Parent).ThenInclude(p => p!.Translations).ThenInclude(t => t.Language)
            .OrderBy(c => c.ParentId).ThenBy(c => c.Id)
            .ToListAsync(ct);
    }

    public async Task<List<Category>> GetRootCategoriesAsync(CancellationToken ct = default)
    {
        return await BaseQuery.Where(c => c.ParentId == null).ToListAsync(ct);
    }

    public async Task<List<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken ct = default)
    {
        return await BaseQuery.Where(c => c.ParentId == parentId).ToListAsync(ct);
    }

    public async Task<Category?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await BaseQuery
            .Include(c => c.Parent).ThenInclude(p => p!.Translations).ThenInclude(t => t.Language)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Categories.AnyAsync(c => c.Id == id, ct);
    }

    public async Task<bool> HasProductsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Products.AnyAsync(p => p.CategoryId == id, ct);
    }

    public async Task<bool> HasSubCategoriesAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Categories.AnyAsync(c => c.ParentId == id, ct);
    }

    public void Add(Category category) => db.Categories.Add(category);
    public void Remove(Category category) => db.Categories.Remove(category);
}
