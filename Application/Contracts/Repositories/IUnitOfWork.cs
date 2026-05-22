namespace NexCommerce.Application.Contracts.Repositories;

/// <summary>
/// Commits all pending changes across all repositories that share the same
/// DbContext instance. Keeps EF Core's SaveChangesAsync out of Application.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
