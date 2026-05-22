using NexCommerce.Application.Common;
using NexCommerce.Application.DTOs.Products;

namespace NexCommerce.Application.Contracts;

public interface IProductService
{
    Task<PagedResult<ProductListItemDto>> GetAllAsync(int page, int pageSize, string lang, CancellationToken ct = default);
    Task<PagedResult<ProductListItemDto>> GetByVendorAsync(Guid vendorId, int page, int pageSize, string lang, CancellationToken ct = default);
    Task<PagedResult<ProductListItemDto>> GetByCategoryAsync(Guid categoryId, int page, int pageSize, string lang, CancellationToken ct = default);
    Task<ProductDetailDto> GetByIdAsync(Guid id, Guid? requestingUserId, string lang, CancellationToken ct = default);

    Task RecordViewAsync(Guid productId, Guid userId, CancellationToken ct = default);
}
