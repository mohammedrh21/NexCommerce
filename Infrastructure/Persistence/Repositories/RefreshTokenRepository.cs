using Microsoft.EntityFrameworkCore;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Domain.Entities;

namespace NexCommerce.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(NexCommerceDbContext db) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> FindValidAsync(Guid userId, string token, CancellationToken ct = default)
    {
        return await db.RefreshTokens
            .FirstOrDefaultAsync(t =>
                t.UserId == userId &&
                t.Token == token &&
                !t.IsRevoked &&
                t.ExpiryDate > DateTime.UtcNow, ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var existingTokens = await db.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);

        foreach (var t in existingTokens)
            t.IsRevoked = true;
    }

    public void Add(RefreshToken token) => db.RefreshTokens.Add(token);
}
