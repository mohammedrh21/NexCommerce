using NexCommerce.Application.Common;
using NexCommerce.Application.DTOs.Coupons;

namespace NexCommerce.Application.Contracts;

public interface ICouponService
{
    Task<PagedResult<CouponDto>> GetAllAsync(int page, int pageSize, string lang, CancellationToken ct = default);
    Task<CouponDto> GetByIdAsync(Guid id, string lang, CancellationToken ct = default);
    Task<CouponDto> CreateAsync(CreateCouponRequest request, CancellationToken ct = default);
    Task<CouponDto> UpdateAsync(Guid id, UpdateCouponRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<CouponValidationResultDto> ValidateAsync(ValidateCouponRequest request, CancellationToken ct = default);
}
