using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Orders;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;
using NexCommerce.Domain.Enums;

namespace NexCommerce.Application.Features.Orders.UpdateOrderStatus;

public record UpdateOrderStatusCommand(
    Guid OrderId,
    Guid RequestingUserId,
    IEnumerable<string> Roles,
    UpdateOrderStatusRequest Request) : IRequest<OrderSummaryDto>;

public sealed class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IVendorRepository vendorRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    INotificationService notificationService,
    IEmailService emailService,
    IIdentityService identityService,
    ILogger<UpdateOrderStatusCommandHandler> logger)
    : IRequestHandler<UpdateOrderStatusCommand, OrderSummaryDto>
{
    // Allowed transitions
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Pending]    = [OrderStatus.Processing, OrderStatus.Cancelled],
        [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Cancelled],
        [OrderStatus.Shipped]    = [OrderStatus.Delivered],
        [OrderStatus.Delivered]  = [],
        [OrderStatus.Cancelled]  = []
    };

    public async Task<OrderSummaryDto> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.FindWithItemsAndHistoryAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Order {command.OrderId} not found.");

        var isAdmin  = command.Roles.Contains("Admin");
        var isVendor = command.Roles.Contains("Vendor");

        // Vendors can only move to Shipped
        if (isVendor && !isAdmin)
        {
            var vendorProfile = await vendorRepository.FindByUserIdAsync(command.RequestingUserId, cancellationToken);

            var ownsItems = order.Items.Any(i => i.VendorId == vendorProfile?.Id);
            if (!ownsItems)
                throw new ForbiddenException("You do not have permission to update this order.");
        }

        var newStatus = command.Request.NewStatus;
        var allowed   = AllowedTransitions.GetValueOrDefault(order.Status, []);

        if (!allowed.Contains(newStatus))
            throw new ValidationException($"Cannot transition order from {order.Status} to {newStatus}.");

        order.Status = newStatus;
        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId   = order.Id,
            Status    = newStatus,
            Note      = command.Request.Note,
            ChangedAt = DateTime.UtcNow
        });

        // Restore stock on cancellation
        if (newStatus == OrderStatus.Cancelled)
        {
            var variantIds = order.Items.Select(i => i.ProductVariantId).ToList();
            var variants = await productRepository.GetVariantsByIdsAsync(variantIds, cancellationToken);

            foreach (var item in order.Items)
            {
                var variant = variants.FirstOrDefault(v => v.Id == item.ProductVariantId);
                if (variant is not null)
                    variant.StockQuantity += item.Quantity;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify customer
        var user = await identityService.GetUserInfoAsync(order.UserId);
        await notificationService.SendAsync(order.UserId, NotificationType.OrderUpdate,
            $"Your order status has been updated to: {newStatus}", cancellationToken);
        _ = emailService.SendOrderStatusUpdateAsync(user.Email, user.FullName, order.Id, newStatus.ToString(), cancellationToken);

        logger.LogInformation("Order {OrderId} status updated to {Status} by {UserId}",
            command.OrderId, newStatus, command.RequestingUserId);

        return new OrderSummaryDto(order.Id, user.FullName, order.TotalAmount, order.Status, order.Items.Count, order.CreatedAt);
    }
}
