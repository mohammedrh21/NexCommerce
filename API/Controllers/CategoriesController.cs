using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Categories;
using NexCommerce.Application.Features.Categories.CreateCategory;
using NexCommerce.Application.Features.Categories.DeleteCategory;
using NexCommerce.Application.Features.Categories.UpdateCategory;

namespace NexCommerce.API.Controllers;

public class CategoriesController(IMediator mediator, ICategoryService categoryService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string lang = "en", CancellationToken ct = default)
    {
        var result = await categoryService.GetAllAsync(lang, ct);
        return Ok(ApiResponse<IEnumerable<CategoryListItemDto>>.Ok(result));
    }

    [HttpGet("root")]
    public async Task<IActionResult> GetRoots([FromQuery] string lang = "en", CancellationToken ct = default)
    {
        var result = await categoryService.GetRootCategoriesAsync(lang, ct);
        return Ok(ApiResponse<IEnumerable<CategoryListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}/subcategories")]
    public async Task<IActionResult> GetSubcategories([FromRoute] Guid id, [FromQuery] string lang = "en", CancellationToken ct = default)
    {
        var result = await categoryService.GetSubCategoriesAsync(id, lang, ct);
        return Ok(ApiResponse<IEnumerable<CategoryListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, [FromQuery] string lang = "en", CancellationToken ct = default)
    {
        var result = await categoryService.GetByIdAsync(id, lang, ct);
        return Ok(ApiResponse<CategoryDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateCategoryCommand(request), ct);
        return Ok(ApiResponse<CategoryDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateCategoryCommand(id, request), ct);
        return Ok(ApiResponse<CategoryDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteCategoryCommand(id), ct);
        return Ok(ApiResponse.OkNoData("Category deleted successfully"));
    }
}
