namespace NexCommerce.Application.Contracts;

/// <summary>Stripe payment operations — implemented in Infrastructure.</summary>
public interface IPaymentService
{
    /// <summary>Create a Stripe PaymentIntent and return the client secret.</summary>
    Task<PaymentIntentResultDto> CreatePaymentIntentAsync(decimal amount, string currency, Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>Confirm that a PaymentIntent completed successfully.</summary>
    Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);

    /// <summary>Issue a full or partial refund.</summary>
    Task<bool> RefundAsync(string paymentIntentId, decimal? amount = null, CancellationToken cancellationToken = default);
}

public record PaymentIntentResultDto(string PaymentIntentId, string ClientSecret);
