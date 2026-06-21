using Client.Models.Languages;

namespace Client.Services.Http;

public interface ILanguagesApiService
{
    Task<List<LanguageDto>> GetAllAsync(CancellationToken ct = default);
    Task<LanguageDto> GetByCodeAsync(string code, CancellationToken ct = default);
}

public class LanguagesApiService : BaseApiService, ILanguagesApiService
{
    public LanguagesApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<List<LanguageDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await GetAsync<List<LanguageDto>>("api/v1/languages", ct);
    }

    public async Task<LanguageDto> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await GetAsync<LanguageDto>($"api/v1/languages/{code}", ct);
    }
}
