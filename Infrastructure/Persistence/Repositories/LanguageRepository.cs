using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Repositories;

public sealed class LanguageRepository(NexCommerceDbContext db) : ILanguageRepository
{
    public async Task<List<Language>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default)
    {
        return await db.Languages
            .Where(l => codes.Contains(l.Code))
            .ToListAsync(ct);
    }

    public async Task<Language?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await db.Languages
            .FirstOrDefaultAsync(l => l.Code == code, ct);
    }

    public async Task<List<Language>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Languages.ToListAsync(ct);
    }
}
