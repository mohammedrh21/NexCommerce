using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Contracts.Repositories;

public interface IOrderRepository
{
    Task<(List<Order> Items, int Total)> GetByUserPagedAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<(List<Order> Items, int Total)> GetByVendorPagedAsync(Guid vendorId, int page, int pageSize, CancellationToken ct = default);
    Task<(List<Order> Items, int Total)> GetAllPagedAsync(int page, int pageSize, CancellationToken ct = default);

    /// <summary>Full load: items + variants + products + status history + payment + coupon.</summary>
    Task<Order?> FindWithDetailsAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>Load: items + status history (for status update command).</summary>
    Task<Order?> FindWithItemsAndHistoryAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>Load: items + payment (for cancel/refund command).</summary>
    Task<Order?> FindWithItemsAndPaymentAsync(Guid orderId, CancellationToken ct = default);

    void Add(Order order);
}
