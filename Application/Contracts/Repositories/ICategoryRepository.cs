using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Contracts.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken ct = default);
    Task<List<Category>> GetRootCategoriesAsync(CancellationToken ct = default);
    Task<List<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken ct = default);
    Task<Category?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasProductsAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasSubCategoriesAsync(Guid id, CancellationToken ct = default);
    void Add(Category category);
    void Remove(Category category);
}
