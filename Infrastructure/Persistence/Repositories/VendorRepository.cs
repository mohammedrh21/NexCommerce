using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Repositories;

public sealed class VendorRepository(NexCommerceDbContext db) : IVendorRepository
{
    public async Task<(List<VendorProfile> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.VendorProfiles
            .Include(v => v.Translations).ThenInclude(t => t.Language)
            .Include(v => v.Products)
            .OrderByDescending(v => v.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<VendorProfile?> FindByIdAsync(Guid vendorId, CancellationToken ct = default)
    {
        return await db.VendorProfiles
            .Include(v => v.Translations).ThenInclude(t => t.Language)
            .Include(v => v.Products)
            .FirstOrDefaultAsync(v => v.Id == vendorId, ct);
    }

    public async Task<VendorProfile?> FindByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.VendorProfiles
            .Include(v => v.Translations).ThenInclude(t => t.Language)
            .Include(v => v.Products)
            .FirstOrDefaultAsync(v => v.UserId == userId, ct);
    }

    public async Task<bool> ExistsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.VendorProfiles.AnyAsync(v => v.UserId == userId, ct);
    }

    public void Add(VendorProfile vendor) => db.VendorProfiles.Add(vendor);
}
