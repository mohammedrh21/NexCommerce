using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Orders;
using NexCommerce.Application.Exceptions;

namespace NexCommerce.Application.Services;

public sealed class OrderService(
    IOrderRepository orderRepository,
    IIdentityService identityService,
    IVendorRepository vendorRepository)
    : IOrderService
{
    public async Task<PagedResult<OrderSummaryDto>> GetCustomerOrdersAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await orderRepository.GetByUserPagedAsync(userId, page, pageSize, ct);
        var dtos = await MapSummariesAsync(items, ct);
        return new PagedResult<OrderSummaryDto> { Data = dtos, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<OrderSummaryDto>> GetVendorOrdersAsync(Guid vendorUserId, int page, int pageSize, CancellationToken ct = default)
    {
        var vendor = await vendorRepository.FindByUserIdAsync(vendorUserId, ct)
            ?? throw new NotFoundException($"Vendor profile not found for user {vendorUserId}.");

        var (items, total) = await orderRepository.GetByVendorPagedAsync(vendor.Id, page, pageSize, ct);
        var dtos = await MapSummariesAsync(items, ct);
        return new PagedResult<OrderSummaryDto> { Data = dtos, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<OrderSummaryDto>> GetAllOrdersAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await orderRepository.GetAllPagedAsync(page, pageSize, ct);
        var dtos = await MapSummariesAsync(items, ct);
        return new PagedResult<OrderSummaryDto> { Data = dtos, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<OrderDto> GetByIdAsync(Guid orderId, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var order = await orderRepository.FindWithDetailsAsync(orderId, ct)
            ?? throw new NotFoundException($"Order {orderId} not found.");

        var roleList = roles.ToList();
        var isAdmin  = roleList.Contains("Admin");
        var isVendor = roleList.Contains("Vendor");

        // Authorization: Admin sees all. Customer sees their own orders.
        // Vendor sees orders only when at least one item in the order belongs to their store.
        if (!isAdmin && order.UserId != requestingUserId)
        {
            if (!isVendor || !order.Items.Any(i => i.VendorId == requestingUserId))
                throw new ForbiddenException("You do not have permission to view this order.");
        }

        var customer = await identityService.GetUserInfoAsync(order.UserId);

        return new OrderDto(
            Id:              order.Id,
            UserId:          order.UserId,
            CustomerName:    customer.FullName,
            CustomerEmail:   customer.Email,
            TotalAmount:     order.TotalAmount,
            DiscountAmount:  null, // Discount is baked into TotalAmount at placement time
            Status:          order.Status,
            ShippingAddress: order.ShippingAddress,
            CreatedAt:       order.CreatedAt,
            CouponId:        order.CouponId,
            CouponCode:      order.Coupon?.Code,
            Items: order.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.Product.Translations.FirstOrDefault()?.Name ?? string.Empty,
                i.Product.Images.FirstOrDefault(img => img.IsMain)?.ImageUrl,
                i.ProductVariantId,
                i.ProductVariant.Size,
                i.ProductVariant.Color,
                i.Quantity,
                i.Price,
                i.Quantity * i.Price)),
            StatusHistory: order.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .Select(h => new OrderStatusHistoryDto(h.Status, h.Note, h.ChangedAt)),
            Payment: order.Payment is not null
                ? new PaymentSummaryDto(
                    order.Payment.Id,
                    order.Payment.Amount,
                    order.Payment.Status,
                    order.Payment.StripePaymentIntentId,
                    order.Payment.PaidAt)
                : null);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Resolves customer full names in bulk, caching per unique userId
    /// to avoid redundant identity service round-trips.
    /// </summary>
    private async Task<IEnumerable<OrderSummaryDto>> MapSummariesAsync(
        IEnumerable<Domain.Entities.Order> orders, CancellationToken ct)
    {
        var orderList = orders.ToList();

        // Build a name cache keyed by UserId
        var nameCache = new Dictionary<Guid, string>();
        foreach (var userId in orderList.Select(o => o.UserId).Distinct())
        {
            var user = await identityService.GetUserInfoAsync(userId);
            nameCache[userId] = user.FullName;
        }

        return orderList.Select(o => new OrderSummaryDto(
            Id:           o.Id,
            CustomerName: nameCache.GetValueOrDefault(o.UserId, "Unknown"),
            TotalAmount:  o.TotalAmount,
            Status:       o.Status,
            ItemCount:    o.Items.Count,
            CreatedAt:    o.CreatedAt));
    }
}
