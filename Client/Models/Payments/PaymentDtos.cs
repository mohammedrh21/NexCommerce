using Client.Models.Common;

namespace Client.Models.Payments;

public record ProcessPaymentRequest(
    Guid OrderId,
    string PaymentMethodId);

public record CreatePaymentIntentRequest(
    Guid OrderId,
    decimal Amount,
    string Currency = "usd");

public record PaymentIntentDto(
    string PaymentIntentId,
    string ClientSecret,
    decimal Amount,
    string Currency);

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    PaymentStatus Status,
    string? StripePaymentIntentId,
    DateTime? PaidAt);
