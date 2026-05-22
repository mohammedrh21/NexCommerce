using NexCommerce.Application.DTOs.Languages;

namespace NexCommerce.Application.Contracts;

public interface ILanguageService
{
    Task<List<LanguageDto>> GetLanguagesAsync(CancellationToken ct = default);
    Task<LanguageDto?> GetLanguageByCodeAsync(string code, CancellationToken ct = default);
}
