using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;
using Stripe;

namespace NexCommerce.Infrastructure.Services;

/// <summary>
/// Stripe payment integration implementing <see cref="IPaymentService"/>.
/// Configure <c>Stripe:SecretKey</c> in appsettings.Development.json (never committed).
/// </summary>
public sealed class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IConfiguration config, ILogger<PaymentService> logger)
    {
        _logger = logger;

        StripeConfiguration.ApiKey =
            config["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");
    }

    /// <inheritdoc/>
    public async Task<PaymentIntentResultDto> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount   = ConvertToCents(amount),
            Currency = currency.ToLowerInvariant(),
            Metadata = new Dictionary<string, string>
            {
                ["orderId"] = orderId.ToString()
            },
        };

        var service = new PaymentIntentService();
        var intent  = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "PaymentIntent {IntentId} created for Order {OrderId} — amount {Amount} {Currency}",
            intent.Id, orderId, amount, currency);

        return new PaymentIntentResultDto(intent.Id, intent.ClientSecret);
    }

    /// <inheritdoc/>
    public async Task<bool> ConfirmPaymentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        var service = new PaymentIntentService();
        var intent  = await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);

        var succeeded = intent.Status == "succeeded";
        _logger.LogInformation(
            "PaymentIntent {IntentId} status: {Status}", paymentIntentId, intent.Status);

        return succeeded;
    }

    /// <inheritdoc/>
    public async Task<bool> RefundAsync(
        string paymentIntentId,
        decimal? amount = null,
        CancellationToken cancellationToken = default)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
            Amount        = amount.HasValue ? ConvertToCents(amount.Value) : null,
        };

        var service = new RefundService();
        var refund  = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Refund {RefundId} issued for PaymentIntent {IntentId} — status: {Status}",
            refund.Id, paymentIntentId, refund.Status);

        return refund.Status is "succeeded" or "pending";
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static long ConvertToCents(decimal amount) => (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
}
