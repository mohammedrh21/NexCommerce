using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Cart;
using NexCommerce.Application.Features.CartFeatures.AddToCart;
using NexCommerce.Application.Features.CartFeatures.ClearCart;
using NexCommerce.Application.Features.CartFeatures.RemoveCartItem;
using NexCommerce.Application.Features.CartFeatures.UpdateCartItem;

namespace NexCommerce.API.Controllers;

[Authorize]
public class CartController(IMediator mediator, ICartService cartService) : ApiControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken ct = default)
    {
        var result = await cartService.GetCartAsync(CurrentUserId, ct);
        return Ok(ApiResponse<CartDto>.Ok(result));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartRequest request, CancellationToken ct)
    {
        await mediator.Send(new AddToCartCommand(CurrentUserId, request), ct);
        var cart = await cartService.GetCartAsync(CurrentUserId, ct);
        return Ok(ApiResponse<CartDto>.Ok(cart));
    }

    [HttpPut("items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem([FromRoute] Guid itemId, [FromBody] UpdateCartItemRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdateCartItemCommand(CurrentUserId, itemId, request), ct);
        var cart = await cartService.GetCartAsync(CurrentUserId, ct);
        return Ok(ApiResponse<CartDto>.Ok(cart));
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem([FromRoute] Guid itemId, CancellationToken ct)
    {
        await mediator.Send(new RemoveCartItemCommand(CurrentUserId, itemId), ct);
        var cart = await cartService.GetCartAsync(CurrentUserId, ct);
        return Ok(ApiResponse<CartDto>.Ok(cart));
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        await mediator.Send(new ClearCartCommand(CurrentUserId), ct);
        return Ok(ApiResponse.OkNoData("Cart cleared successfully"));
    }
}
