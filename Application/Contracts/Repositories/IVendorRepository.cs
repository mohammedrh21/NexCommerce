using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Contracts.Repositories;

public interface IVendorRepository
{
    Task<(List<VendorProfile> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<VendorProfile?> FindByIdAsync(Guid vendorId, CancellationToken ct = default);
    Task<VendorProfile?> FindByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ExistsForUserAsync(Guid userId, CancellationToken ct = default);
    void Add(VendorProfile vendor);
}
