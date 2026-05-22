using NexCommerce.Application.Common;
using NexCommerce.Application.DTOs.Orders;

namespace NexCommerce.Application.Contracts;

public interface IOrderService
{
    Task<PagedResult<OrderSummaryDto>> GetCustomerOrdersAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<OrderSummaryDto>> GetVendorOrdersAsync(Guid vendorUserId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<OrderSummaryDto>> GetAllOrdersAsync(int page, int pageSize, CancellationToken ct = default);
    Task<OrderDto> GetByIdAsync(Guid orderId, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);
}
