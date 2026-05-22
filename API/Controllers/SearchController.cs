using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Search;

namespace NexCommerce.API.Controllers;

public class SearchController(ISearchService searchService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] SearchRequest request,
        [FromQuery] string lang = "en",
        CancellationToken ct = default)
    {
        var result = await searchService.SearchAsync(request, CurrentUserIdNullable, lang, ct);
        return Ok(ApiResponse<SearchResultDto>.Ok(result));
    }
}
