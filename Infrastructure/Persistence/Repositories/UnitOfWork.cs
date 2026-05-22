using NexCommerce.Application.Contracts.Repositories;

namespace NexCommerce.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork(NexCommerceDbContext db) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await db.SaveChangesAsync(cancellationToken);
    }
}
