using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Payments;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Enums;

namespace NexCommerce.Application.Features.Payments.ProcessPayment;

public record ProcessPaymentCommand(Guid OrderId, Guid UserId, string PaymentIntentId) : IRequest<PaymentDto>;

public sealed class ProcessPaymentCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IPaymentService paymentService,
    INotificationService notificationService,
    ILogger<ProcessPaymentCommandHandler> logger)
    : IRequestHandler<ProcessPaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.FindWithItemsAndPaymentAsync(command.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Order {command.OrderId} not found.");

        if (order.UserId != command.UserId)
            throw new ForbiddenException("You can only pay for your own orders.");

        if (order.Payment is null)
            throw new NotFoundException("Payment record not found for this order.");

        if (order.Payment.Status == PaymentStatus.Succeeded)
            throw new ValidationException("Order is already paid.");

        var confirmed = await paymentService.ConfirmPaymentAsync(command.PaymentIntentId, cancellationToken);
        if (!confirmed)
            throw new ValidationException("Payment confirmation failed. Please try again.");

        order.Payment.Status    = PaymentStatus.Succeeded;
        order.Payment.PaidAt    = DateTime.UtcNow;
        order.Status            = OrderStatus.Processing;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(order.UserId, NexCommerce.Domain.Enums.NotificationType.OrderUpdate,
            $"Payment confirmed for order #{order.Id.ToString()[..8]}.", cancellationToken);

        logger.LogInformation("Payment confirmed for order {OrderId}", command.OrderId);

        return new PaymentDto(order.Payment.Id, order.Id, order.Payment.Amount,
            order.Payment.Status, order.Payment.StripePaymentIntentId, order.Payment.PaidAt);
    }
}
