using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Contracts.Repositories;

public interface ICouponRepository
{
    Task<(List<Coupon> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Coupon?> FindByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Finds coupon by code including usages for count validation.</summary>
    Task<Coupon?> FindByCodeAsync(string code, CancellationToken ct = default);

    Task<int> GetUsageCountAsync(Guid couponId, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    void Add(Coupon coupon);
    void Remove(Coupon coupon);
    void AddUsage(CouponUsage usage);
}
