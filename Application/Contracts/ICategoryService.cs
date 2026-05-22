using NexCommerce.Application.Common;
using NexCommerce.Application.DTOs.Categories;

namespace NexCommerce.Application.Contracts;

public interface ICategoryService
{
    Task<IEnumerable<CategoryListItemDto>> GetAllAsync(string lang, CancellationToken ct = default);
    Task<IEnumerable<CategoryListItemDto>> GetRootCategoriesAsync(string lang, CancellationToken ct = default);
    Task<IEnumerable<CategoryListItemDto>> GetSubCategoriesAsync(Guid parentId, string lang, CancellationToken ct = default);
    Task<CategoryDto> GetByIdAsync(Guid id, string lang, CancellationToken ct = default);

}
