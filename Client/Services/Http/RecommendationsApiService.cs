using Client.Models.Recommendations;

namespace Client.Services.Http;

public interface IRecommendationsApiService
{
    Task<IEnumerable<RecommendationDto>> GetSimilarAsync(Guid productId, int count = 5, string lang = "en", CancellationToken ct = default);
    Task<IEnumerable<RecommendationDto>> GetTrendingAsync(int count = 5, string lang = "en", CancellationToken ct = default);
    Task<IEnumerable<RecommendationDto>> GetPersonalizedAsync(int count = 5, string lang = "en", CancellationToken ct = default);
}

public class RecommendationsApiService : BaseApiService, IRecommendationsApiService
{
    public RecommendationsApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IEnumerable<RecommendationDto>> GetSimilarAsync(Guid productId, int count = 5, string lang = "en", CancellationToken ct = default)
    {
        return await GetAsync<IEnumerable<RecommendationDto>>($"api/v1/recommendations/similar/{productId}?count={count}&lang={lang}", ct);
    }

    public async Task<IEnumerable<RecommendationDto>> GetTrendingAsync(int count = 5, string lang = "en", CancellationToken ct = default)
    {
        return await GetAsync<IEnumerable<RecommendationDto>>($"api/v1/recommendations/trending?count={count}&lang={lang}", ct);
    }

    public async Task<IEnumerable<RecommendationDto>> GetPersonalizedAsync(int count = 5, string lang = "en", CancellationToken ct = default)
    {
        return await GetAsync<IEnumerable<RecommendationDto>>($"api/v1/recommendations/personalized?count={count}&lang={lang}", ct);
    }
}
