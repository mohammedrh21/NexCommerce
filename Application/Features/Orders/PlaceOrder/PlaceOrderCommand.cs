using MediatR;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Orders;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;
using NexCommerce.Domain.Enums;

namespace NexCommerce.Application.Features.Orders.PlaceOrder;

public record PlaceOrderCommand(
    Guid UserId,
    PlaceOrderRequest Request) : IRequest<OrderDto>;

public sealed class PlaceOrderCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    ICouponRepository couponRepository,
    IUnitOfWork unitOfWork,
    ICouponService couponService,
    IPaymentService paymentService,
    INotificationService notificationService,
    IEmailService emailService,
    IIdentityService identityService,
    ILogger<PlaceOrderCommandHandler> logger)
    : IRequestHandler<PlaceOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        // Load variants with product info
        var variantIds = req.Items.Select(i => i.ProductVariantId).ToList();
        var variants = await productRepository.GetVariantsByIdsAsync(variantIds, cancellationToken);

        // Validate all variants found
        foreach (var item in req.Items)
        {
            var variant = variants.FirstOrDefault(v => v.Id == item.ProductVariantId)
                ?? throw new NotFoundException($"Product variant {item.ProductVariantId} not found.");

            if (variant.StockQuantity < item.Quantity)
                throw new ValidationException($"Insufficient stock for variant {variant.SKU ?? variant.Id.ToString()}");
        }

        // Validate & apply coupon
        Guid? couponId = null;
        decimal discount = 0;
        decimal rawTotal = req.Items.Sum(item =>
        {
            var variant = variants.First(v => v.Id == item.ProductVariantId);
            var price = variant.Product.Price + (variant.PriceAdjustment ?? 0);
            return price * item.Quantity;
        });

        decimal totalAmount = rawTotal;

        if (!string.IsNullOrWhiteSpace(req.CouponCode))
        {
            var couponResult = await couponService.ValidateAsync(
                new DTOs.Coupons.ValidateCouponRequest(req.CouponCode, rawTotal), cancellationToken);

            if (!couponResult.IsValid)
                throw new ValidationException(couponResult.ErrorMessage ?? "Invalid coupon.");

            couponId     = couponResult.CouponId;
            discount     = couponResult.DiscountAmount ?? 0;
            totalAmount  = couponResult.FinalAmount ?? rawTotal;

            // Record usage
            if (couponId.HasValue)
                couponRepository.AddUsage(new CouponUsage { CouponId = couponId.Value, UserId = command.UserId, UsedAt = DateTime.UtcNow });
        }

        // Create Order
        var order = new Order
        {
            Id          = Guid.NewGuid(),
            UserId      = command.UserId,
            TotalAmount = totalAmount,
            Status      = OrderStatus.Pending,
            CouponId    = couponId,
            CreatedAt   = DateTime.UtcNow
        };

        order.Items = req.Items.Select(item =>
        {
            var variant = variants.First(v => v.Id == item.ProductVariantId);
            var price = variant.Product.Price + (variant.PriceAdjustment ?? 0);
            return new OrderItem
            {
                Id               = Guid.NewGuid(),
                OrderId          = order.Id,
                ProductVariantId = item.ProductVariantId,
                ProductId        = variant.ProductId,
                VendorId         = variant.Product.VendorId,
                Quantity         = item.Quantity,
                Price            = price
            };
        }).ToList();

        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId   = order.Id,
            Status    = OrderStatus.Pending,
            Note      = "Order placed",
            ChangedAt = DateTime.UtcNow
        });

        // Deduct stock
        foreach (var item in req.Items)
        {
            var variant = variants.First(v => v.Id == item.ProductVariantId);
            variant.StockQuantity -= item.Quantity;
        }

        // Create Stripe PaymentIntent
        var paymentIntent = await paymentService.CreatePaymentIntentAsync(
            totalAmount, "usd", order.Id, cancellationToken);

        order.Payment = new Payment
        {
            Id                   = Guid.NewGuid(),
            OrderId              = order.Id,
            Amount               = totalAmount,
            Status               = PaymentStatus.Pending,
            StripePaymentIntentId = paymentIntent.PaymentIntentId
        };

        orderRepository.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Notifications
        var user = await identityService.GetUserInfoAsync(command.UserId);
        _ = emailService.SendOrderConfirmationAsync(user.Email, user.FullName, order.Id, cancellationToken);
        await notificationService.SendAsync(command.UserId, NotificationType.OrderUpdate,
            $"Your order #{order.Id.ToString()[..8]} has been placed.", cancellationToken);

        logger.LogInformation("Order {OrderId} placed by user {UserId}, total {Total}", order.Id, command.UserId, totalAmount);

        // Map response
        var userInfo = await identityService.GetUserInfoAsync(command.UserId);
        return MapOrder(order, variants, userInfo, req.ShippingAddress, discount, paymentIntent.ClientSecret);
    }

    private static OrderDto MapOrder(Order order, List<ProductVariant> variants, UserInfoDto user,
        string shippingAddress, decimal discount, string clientSecret) => new(
        Id:             order.Id,
        UserId:         order.UserId,
        CustomerName:   user.FullName,
        CustomerEmail:  user.Email,
        TotalAmount:    order.TotalAmount,
        DiscountAmount: discount > 0 ? discount : null,
        Status:         order.Status,
        ShippingAddress: shippingAddress,
        CreatedAt:      order.CreatedAt,
        CouponId:       order.CouponId,
        CouponCode:     null,
        Items: order.Items.Select(item =>
        {
            var v = variants.First(x => x.Id == item.ProductVariantId);
            return new OrderItemDto(
                item.Id, item.ProductId,
                v.Product.Translations.FirstOrDefault(t => t.Language.Code == "en")?.Name ?? string.Empty,
                v.Product.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl,
                item.ProductVariantId, v.Size, v.Color,
                item.Quantity, item.Price, item.Price * item.Quantity);
        }),
        StatusHistory: order.StatusHistory.Select(h => new OrderStatusHistoryDto(h.Status, h.Note, h.ChangedAt)),
        Payment: order.Payment is not null
            ? new PaymentSummaryDto(order.Payment.Id, order.Payment.Amount,
                order.Payment.Status, clientSecret, order.Payment.PaidAt)
            : null);
}
