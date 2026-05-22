using NexCommerce.Domain.Enums;

namespace NexCommerce.Application.DTOs.Payments;

// ── Requests ─────────────────────────────────────────────────────────────────

public record ProcessPaymentRequest(
    Guid OrderId,
    string PaymentMethodId);

public record CreatePaymentIntentRequest(
    Guid OrderId,
    decimal Amount,
    string Currency = "usd");

// ── Response DTOs ─────────────────────────────────────────────────────────────

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
