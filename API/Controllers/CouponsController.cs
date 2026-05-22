using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.DTOs.Coupons;

namespace NexCommerce.API.Controllers;

public class CouponsController(ICouponService couponService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        [FromQuery] string lang = "en", 
        CancellationToken ct = default)
    {
        var result = await couponService.GetAllAsync(page, pageSize, lang, ct);
        return Ok(ApiResponse<PagedResult<CouponDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id, 
        [FromQuery] string lang = "en", 
        CancellationToken ct = default)
    {
        var result = await couponService.GetByIdAsync(id, lang, ct);
        return Ok(ApiResponse<CouponDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Vendor")]
    public async Task<IActionResult> Create(
        [FromBody] CreateCouponRequest request, 
        CancellationToken ct = default)
    {
        var result = await couponService.CreateAsync(request, ct);
        return Ok(ApiResponse<CouponDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Vendor")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id, 
        [FromBody] UpdateCouponRequest request, 
        CancellationToken ct = default)
    {
        var result = await couponService.UpdateAsync(id, request, ct);
        return Ok(ApiResponse<CouponDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Vendor")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id, 
        CancellationToken ct = default)
    {
        await couponService.DeleteAsync(id, ct);
        return Ok(ApiResponse.OkNoData("Coupon deleted successfully."));
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate(
        [FromBody] ValidateCouponRequest request, 
        CancellationToken ct = default)
    {
        var result = await couponService.ValidateAsync(request, ct);
        return Ok(ApiResponse<CouponValidationResultDto>.Ok(result));
    }
}
