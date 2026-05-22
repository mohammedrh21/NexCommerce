using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Vendors;
using NexCommerce.Application.Features.Vendors.ApproveVendor;
using NexCommerce.Application.Features.Vendors.CreateProfile;
using NexCommerce.Application.Features.Vendors.UpdateProfile;

namespace NexCommerce.API.Controllers;

public class VendorsController(IMediator mediator, IVendorService vendorService) : ApiControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string lang = "en",
        CancellationToken ct = default)
    {
        var result = await vendorService.GetAllAsync(page, pageSize, lang, ct);
        return Ok(ApiResponse<PagedResult<VendorListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, [FromQuery] string lang = "en", CancellationToken ct = default)
    {
        var result = await vendorService.GetByIdAsync(id, lang, ct);
        return Ok(ApiResponse<VendorProfileDto>.Ok(result));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile([FromQuery] string lang = "en", CancellationToken ct = default)
    {
        var result = await vendorService.GetByUserIdAsync(CurrentUserId, lang, ct);
        return Ok(ApiResponse<VendorProfileDto>.Ok(result));
    }

    [HttpGet("{id:guid}/stats")]
    [Authorize(Roles = "Vendor,Admin")]
    public async Task<IActionResult> GetStats([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await vendorService.GetStatsAsync(id, ct);
        return Ok(ApiResponse<VendorStatsDto>.Ok(result));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateProfile([FromBody] CreateVendorProfileRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateVendorProfileCommand(CurrentUserId, request), ct);
        return Ok(ApiResponse<VendorProfileDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> UpdateProfile([FromRoute] Guid id, [FromBody] UpdateVendorProfileRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateVendorProfileCommand(id, CurrentUserId, request), ct);
        return Ok(ApiResponse<VendorProfileDto>.Ok(result));
    }

    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveVendor([FromRoute] Guid id, [FromBody] bool isApproved, CancellationToken ct)
    {
        var result = await mediator.Send(new ApproveVendorCommand(id, isApproved), ct);
        return Ok(ApiResponse<VendorProfileDto>.Ok(result));
    }
}
