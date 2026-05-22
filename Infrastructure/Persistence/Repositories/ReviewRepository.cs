using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Repositories;

public sealed class ReviewRepository(NexCommerceDbContext db) : IReviewRepository
{
    public async Task<(List<Review> Items, int Total)> GetByProductPagedAsync(Guid productId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Review?> FindByIdAsync(Guid reviewId, CancellationToken ct = default)
    {
        return await db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, ct);
    }

    public async Task<decimal> GetAverageRatingAsync(Guid productId, CancellationToken ct = default)
    {
        var ratings = await db.Reviews
            .Where(r => r.ProductId == productId)
            .Select(r => r.Rating)
            .ToListAsync(ct);

        if (ratings.Count == 0) return 0;
        return (decimal)ratings.Average(r => r);
    }

    public async Task<bool> HasUserReviewedAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        return await db.Reviews.AnyAsync(r => r.UserId == userId && r.ProductId == productId, ct);
    }

    public void Add(Review review) => db.Reviews.Add(review);
    public void Remove(Review review) => db.Reviews.Remove(review);
}
