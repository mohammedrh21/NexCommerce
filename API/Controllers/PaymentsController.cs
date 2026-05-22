using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Payments;
using NexCommerce.Application.Features.Payments.ProcessPayment;

namespace NexCommerce.API.Controllers;

[Authorize]
public class PaymentsController(IMediator mediator, IPaymentService paymentService) : ApiControllerBase
{

    /// <summary>
    /// Creates a Stripe PaymentIntent for the given order, returning a client secret
    /// that the frontend uses to confirm the payment via Stripe.js.
    /// </summary>
    [HttpPost("create-intent")]
    public async Task<IActionResult> CreateIntent([FromBody] CreatePaymentIntentRequest request, CancellationToken ct)
    {
        var intent = await paymentService.CreatePaymentIntentAsync(
            request.Amount, request.Currency, request.OrderId, ct);

        return Ok(ApiResponse<PaymentIntentDto>.Ok(
            new PaymentIntentDto(intent.PaymentIntentId, intent.ClientSecret, request.Amount, request.Currency)));
    }

    /// <summary>
    /// Confirms a completed Stripe payment, marks the order as Processing,
    /// and sends an order-update notification to the customer.
    /// </summary>
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ProcessPaymentRequest request, CancellationToken ct)
    {
        // We derive the PaymentIntentId from the order in the command handler
        var result = await mediator.Send(
            new ProcessPaymentCommand(request.OrderId, CurrentUserId, request.PaymentMethodId), ct);

        return Ok(ApiResponse<PaymentDto>.Ok(result));
    }
}
