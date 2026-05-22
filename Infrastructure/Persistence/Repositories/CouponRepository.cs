using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Repositories;

public sealed class CouponRepository(NexCommerceDbContext db) : ICouponRepository
{
    public async Task<(List<Coupon> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Coupons
            .Include(c => c.Translations).ThenInclude(t => t.Language)
            .Include(c => c.Usages)
            .OrderByDescending(c => c.Id);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Coupon?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Coupons
            .Include(c => c.Translations).ThenInclude(t => t.Language)
            .Include(c => c.Usages)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Coupon?> FindByCodeAsync(string code, CancellationToken ct = default)
    {
        return await db.Coupons
            .Include(c => c.Usages)
            .FirstOrDefaultAsync(c => c.Code == code, ct);
    }

    public async Task<int> GetUsageCountAsync(Guid couponId, CancellationToken ct = default)
    {
        return await db.CouponUsages.CountAsync(u => u.CouponId == couponId, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        return await db.Coupons.AnyAsync(c => c.Code == code, ct);
    }

    public void Add(Coupon coupon) => db.Coupons.Add(coupon);
    public void Remove(Coupon coupon) => db.Coupons.Remove(coupon);
    public void AddUsage(CouponUsage usage) => db.CouponUsages.Add(usage);
}
