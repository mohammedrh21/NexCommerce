using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;
using NexCommerce.Domain.Enums;

namespace NexCommerce.Application.Features.Orders.CancelOrder;

public record CancelOrderCommand(Guid OrderId, Guid UserId, string Reason) : IRequest;

public sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IPaymentService paymentService,
    INotificationService notificationService,
    ILogger<CancelOrderCommandHandler> logger)
    : IRequestHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.FindWithDetailsAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Order {command.OrderId} not found.");

        if (order.UserId != command.UserId)
            throw new ForbiddenException("You can only cancel your own orders.");

        if (order.Status is not (OrderStatus.Pending or OrderStatus.Processing))
            throw new ValidationException($"Order cannot be cancelled in {order.Status} status.");

        order.Status = OrderStatus.Cancelled;
        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId   = order.Id,
            Status    = OrderStatus.Cancelled,
            Note      = command.Reason,
            ChangedAt = DateTime.UtcNow
        });

        // Restore stock
        var variantIds = order.Items.Select(i => i.ProductVariantId).ToList();
        var variants = await productRepository.GetVariantsByIdsAsync(variantIds, cancellationToken);

        foreach (var item in order.Items)
        {
            var variant = variants.FirstOrDefault(v => v.Id == item.ProductVariantId);
            if (variant is not null)
                variant.StockQuantity += item.Quantity;
        }

        // Refund if already paid
        if (order.Payment is { Status: PaymentStatus.Succeeded, StripePaymentIntentId: not null })
        {
            await paymentService.RefundAsync(order.Payment.StripePaymentIntentId, cancellationToken: cancellationToken);
            order.Payment.Status = PaymentStatus.Refunded;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(command.UserId, NotificationType.OrderUpdate,
            $"Your order #{order.Id.ToString()[..8]} has been cancelled.", cancellationToken);

        logger.LogInformation("Order {OrderId} cancelled by user {UserId}", command.OrderId, command.UserId);
    }
}
