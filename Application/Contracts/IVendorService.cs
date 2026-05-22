using NexCommerce.Application.Common;
using NexCommerce.Application.DTOs.Vendors;

namespace NexCommerce.Application.Contracts;

public interface IVendorService
{
    Task<PagedResult<VendorListItemDto>> GetAllAsync(int page, int pageSize, string lang, CancellationToken ct = default);
    Task<VendorProfileDto> GetByIdAsync(Guid vendorId, string lang, CancellationToken ct = default);
    Task<VendorProfileDto> GetByUserIdAsync(Guid userId, string lang, CancellationToken ct = default);
    Task<VendorStatsDto> GetStatsAsync(Guid vendorId, CancellationToken ct = default);
}
