using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Reviews;

namespace NexCommerce.API.Controllers;

public class ReviewsController(IReviewService reviewService) : ApiControllerBase
{
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetProductReviews(
        [FromRoute] Guid productId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        CancellationToken ct = default)
    {
        var result = await reviewService.GetProductReviewsAsync(productId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<ReviewDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await reviewService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ReviewDto>.Ok(result));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateReviewRequest request, 
        CancellationToken ct = default)
    {
        var result = await reviewService.CreateAsync(CurrentUserId, request, ct);
        return Ok(ApiResponse<ReviewDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct = default)
    {
        await reviewService.DeleteAsync(id, CurrentUserId, CurrentUserRoles.ToList(), ct);
        return Ok(ApiResponse.OkNoData("Review deleted successfully."));
    }
}
