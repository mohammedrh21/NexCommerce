using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(NexCommerceDbContext db) : IOrderRepository
{
    private IQueryable<Order> BaseQuery => db.Orders
        .Include(o => o.Items)
        .Include(o => o.StatusHistory)
        .Include(o => o.Payment)
        .Include(o => o.Coupon);

    public async Task<(List<Order> Items, int Total)> GetByUserPagedAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = BaseQuery.Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(List<Order> Items, int Total)> GetByVendorPagedAsync(Guid vendorId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = BaseQuery.Where(o => o.Items.Any(i => i.VendorId == vendorId)).OrderByDescending(o => o.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(List<Order> Items, int Total)> GetAllPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = BaseQuery.OrderByDescending(o => o.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Order?> FindWithDetailsAsync(Guid orderId, CancellationToken ct = default)
    {
        return await BaseQuery.FirstOrDefaultAsync(o => o.Id == orderId, ct);
    }

    public async Task<Order?> FindWithItemsAndHistoryAsync(Guid orderId, CancellationToken ct = default)
    {
        return await db.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
    }

    public async Task<Order?> FindWithItemsAndPaymentAsync(Guid orderId, CancellationToken ct = default)
    {
        return await db.Orders
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
    }

    public void Add(Order order) => db.Orders.Add(order);
}
