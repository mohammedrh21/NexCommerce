using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Recommendations;

namespace NexCommerce.API.Controllers;

public class RecommendationsController(IRecommendationService recommendationService) : ApiControllerBase
{

    [HttpGet("similar/{productId:guid}")]
    public async Task<IActionResult> GetSimilar(
        [FromRoute] Guid productId,
        [FromQuery] int count = 5,
        [FromQuery] string lang = "en",
        CancellationToken ct = default)
    {
        var result = await recommendationService.GetSimilarProductsAsync(productId, count, lang, ct);
        return Ok(ApiResponse<IEnumerable<RecommendationDto>>.Ok(result));
    }

    [HttpGet("trending")]
    public async Task<IActionResult> GetTrending(
        [FromQuery] int count = 5,
        [FromQuery] string lang = "en",
        CancellationToken ct = default)
    {
        var result = await recommendationService.GetTrendingAsync(count, lang, ct);
        return Ok(ApiResponse<IEnumerable<RecommendationDto>>.Ok(result));
    }

    [HttpGet("personalized")]
    [Authorize]
    public async Task<IActionResult> GetPersonalized(
        [FromQuery] int count = 5,
        [FromQuery] string lang = "en",
        CancellationToken ct = default)
    {
        var result = await recommendationService.GetPersonalizedAsync(CurrentUserId, count, lang, ct);
        return Ok(ApiResponse<IEnumerable<RecommendationDto>>.Ok(result));
    }
}
