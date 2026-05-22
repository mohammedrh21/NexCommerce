using NexCommerce.Domain.Enums;

namespace NexCommerce.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>Stripe PaymentIntent ID for reconciliation and refunds.</summary>
    public string? StripePaymentIntentId { get; set; }

    public DateTime? PaidAt { get; set; }

    // FK (unique — one payment per order)
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
