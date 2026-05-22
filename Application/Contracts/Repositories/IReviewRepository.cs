using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Contracts.Repositories;

public interface IReviewRepository
{
    Task<(List<Review> Items, int Total)> GetByProductPagedAsync(Guid productId, int page, int pageSize, CancellationToken ct = default);
    Task<Review?> FindByIdAsync(Guid reviewId, CancellationToken ct = default);
    Task<decimal> GetAverageRatingAsync(Guid productId, CancellationToken ct = default);
    Task<bool> HasUserReviewedAsync(Guid userId, Guid productId, CancellationToken ct = default);
    void Add(Review review);
    void Remove(Review review);
}
