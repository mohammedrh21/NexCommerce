using NexCommerce.Domain.Entities;

namespace NexCommerce.Application.Contracts.Repositories;

public interface ILanguageRepository
{
    Task<List<Language>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default);
    Task<Language?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<List<Language>> GetAllAsync(CancellationToken ct = default);
}
