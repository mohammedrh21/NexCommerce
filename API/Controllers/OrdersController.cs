using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Orders;
using NexCommerce.Application.Features.Orders.CancelOrder;
using NexCommerce.Application.Features.Orders.PlaceOrder;
using NexCommerce.Application.Features.Orders.UpdateOrderStatus;

namespace NexCommerce.API.Controllers;

[Authorize]
public class OrdersController(IMediator mediator, IOrderService orderService) : ApiControllerBase
{

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await orderService.GetCustomerOrdersAsync(CurrentUserId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<OrderSummaryDto>>.Ok(result));
    }

    [HttpGet("vendor")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> GetVendorOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await orderService.GetVendorOrdersAsync(CurrentUserId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<OrderSummaryDto>>.Ok(result));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await orderService.GetAllOrdersAsync(page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<OrderSummaryDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await orderService.GetByIdAsync(id, CurrentUserId, CurrentUserRoles, ct);
        return Ok(ApiResponse<OrderDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new PlaceOrderCommand(CurrentUserId, request), ct);
        return Ok(ApiResponse<OrderDto>.Ok(result));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Vendor,Admin")]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateOrderStatusCommand(id, CurrentUserId, CurrentUserRoles, request), ct);
        return Ok(ApiResponse<OrderSummaryDto>.Ok(result));
    }

    [HttpDelete("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder([FromRoute] Guid id, [FromBody] CancelOrderRequest request, CancellationToken ct)
    {
        await mediator.Send(new CancelOrderCommand(id, CurrentUserId, request.Reason), ct);
        return Ok(ApiResponse.OkNoData("Order cancelled successfully"));
    }
}
