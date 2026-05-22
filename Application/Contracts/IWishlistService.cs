using NexCommerce.Application.Common;
using NexCommerce.Application.DTOs.Products;

namespace NexCommerce.Application.Contracts;

public interface IWishlistService
{
    Task<PagedResult<ProductListItemDto>> GetWishlistAsync(Guid userId, int page, int pageSize, string lang, CancellationToken ct = default);
    Task AddToWishlistAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task RemoveFromWishlistAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task<bool> IsInWishlistAsync(Guid userId, Guid productId, CancellationToken ct = default);
}
