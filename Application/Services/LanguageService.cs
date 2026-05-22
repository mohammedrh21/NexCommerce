using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Languages;

namespace NexCommerce.Application.Services;

public sealed class LanguageService(ILanguageRepository languageRepository) : ILanguageService
{
    public async Task<List<LanguageDto>> GetLanguagesAsync(CancellationToken ct = default)
    {
        var languages = await languageRepository.GetAllAsync(ct);
        return languages.Select(l => new LanguageDto(l.Id, l.Code, l.Name, l.IsDefault)).ToList();
    }

    public async Task<LanguageDto?> GetLanguageByCodeAsync(string code, CancellationToken ct = default)
    {
        var language = await languageRepository.GetByCodeAsync(code, ct);
        if (language == null) return null;
        return new LanguageDto(language.Id, language.Code, language.Name, language.IsDefault);
    }
}
