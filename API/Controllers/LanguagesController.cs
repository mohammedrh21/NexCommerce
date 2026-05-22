using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Languages;

namespace NexCommerce.API.Controllers;

public class LanguagesController(ILanguageService languageService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await languageService.GetLanguagesAsync(ct);
        return Ok(ApiResponse<List<LanguageDto>>.Ok(result));
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode([FromRoute] string code, CancellationToken ct = default)
    {
        var result = await languageService.GetLanguageByCodeAsync(code, ct);
        if (result == null)
        {
            return NotFound(ApiResponse<LanguageDto>.Fail($"Language with code '{code}' was not found."));
        }
        return Ok(ApiResponse<LanguageDto>.Ok(result));
    }
}
