using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Products;

namespace NexCommerce.API.Controllers;

[Authorize]
public class WishlistsController(IWishlistService wishlistService) : ApiControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetWishlist(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        [FromQuery] string lang = "en", 
        CancellationToken ct = default)
    {
        var result = await wishlistService.GetWishlistAsync(CurrentUserId, page, pageSize, lang, ct);
        return Ok(ApiResponse<PagedResult<ProductListItemDto>>.Ok(result));
    }

    [HttpPost("{productId:guid}")]
    public async Task<IActionResult> AddToWishlist(
        [FromRoute] Guid productId, 
        CancellationToken ct = default)
    {
        await wishlistService.AddToWishlistAsync(CurrentUserId, productId, ct);
        return Ok(ApiResponse.OkNoData("Product added to wishlist successfully."));
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> RemoveFromWishlist(
        [FromRoute] Guid productId, 
        CancellationToken ct = default)
    {
        await wishlistService.RemoveFromWishlistAsync(CurrentUserId, productId, ct);
        return Ok(ApiResponse.OkNoData("Product removed from wishlist successfully."));
    }

    [HttpGet("{productId:guid}/check")]
    public async Task<IActionResult> CheckWishlist(
        [FromRoute] Guid productId, 
        CancellationToken ct = default)
    {
        var result = await wishlistService.IsInWishlistAsync(CurrentUserId, productId, ct);
        return Ok(ApiResponse<bool>.Ok(result));
    }
}
