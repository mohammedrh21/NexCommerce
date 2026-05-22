using NexCommerce.Application.Common;
using NexCommerce.Application.DTOs.Reviews;

namespace NexCommerce.Application.Contracts;

public interface IReviewService
{
    Task<PagedResult<ReviewDto>> GetProductReviewsAsync(Guid productId, int page, int pageSize, CancellationToken ct = default);
    Task<ReviewDto> GetByIdAsync(Guid reviewId, CancellationToken ct = default);
    Task<ReviewDto> CreateAsync(Guid userId, CreateReviewRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid reviewId, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);
}
