using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Contracts.Repositories;

public interface IRefreshTokenRepository
{
    /// <summary>Returns a valid (non-revoked, non-expired) token for the user.</summary>
    Task<RefreshToken?> FindValidAsync(Guid userId, string token, CancellationToken ct = default);

    /// <summary>Revokes all active refresh tokens for a user (on login to prevent token reuse).</summary>
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);

    void Add(RefreshToken token);
}
