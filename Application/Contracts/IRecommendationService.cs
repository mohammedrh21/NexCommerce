using NexCommerce.Application.DTOs.Recommendations;

namespace NexCommerce.Application.Contracts;

public interface IRecommendationService
{
    /// <summary>"Customers who viewed this also viewed"</summary>
    Task<IEnumerable<RecommendationDto>> GetSimilarProductsAsync(Guid productId, int count, string lang, CancellationToken ct = default);

    /// <summary>"Recommended for you" based on the user's view history.</summary>
    Task<IEnumerable<RecommendationDto>> GetPersonalizedAsync(Guid userId, int count, string lang, CancellationToken ct = default);

    /// <summary>Trending / bestsellers for anonymous users.</summary>
    Task<IEnumerable<RecommendationDto>> GetTrendingAsync(int count, string lang, CancellationToken ct = default);
}
