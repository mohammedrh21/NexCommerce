using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Products;
using NexCommerce.Application.Features.Products.CreateProduct;
using NexCommerce.Application.Features.Products.DeleteProduct;
using NexCommerce.Application.Features.Products.UpdateProduct;

namespace NexCommerce.API.Controllers;

public class ProductsController(IMediator mediator, IProductService productService) : ApiControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string lang = "en",
        CancellationToken ct = default)
    {
        var result = await productService.GetAllAsync(page, pageSize, lang, ct);
        return Ok(ApiResponse<PagedResult<ProductListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, [FromQuery] string lang = "en", CancellationToken ct = default)
    {
        var result = await productService.GetByIdAsync(id, CurrentUserIdNullable, lang, ct);
        return Ok(ApiResponse<ProductDetailDto>.Ok(result));
    }

    [HttpGet("vendor/{vendorId:guid}")]
    public async Task<IActionResult> GetByVendor(
        [FromRoute] Guid vendorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string lang = "en",
        CancellationToken ct = default)
    {
        var result = await productService.GetByVendorAsync(vendorId, page, pageSize, lang, ct);
        return Ok(ApiResponse<PagedResult<ProductListItemDto>>.Ok(result));
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(
        [FromRoute] Guid categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string lang = "en",
        CancellationToken ct = default)
    {
        var result = await productService.GetByCategoryAsync(categoryId, page, pageSize, lang, ct);
        return Ok(ApiResponse<PagedResult<ProductListItemDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Vendor,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateProductCommand(CurrentUserId, request), ct);
        return Ok(ApiResponse<ProductDetailDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Vendor,Admin")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateProductCommand(id, CurrentUserId, IsAdmin, request), ct);
        return Ok(ApiResponse<ProductDetailDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Vendor,Admin")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(id, CurrentUserId, IsAdmin), ct);
        return Ok(ApiResponse.OkNoData("Product deleted successfully"));
    }
}
