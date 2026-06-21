using Client.Models.Common;
using Client.Models.Reviews;

namespace Client.Services.Http;

public interface IReviewsApiService
{
    Task<PagedResult<ReviewDto>> GetProductReviewsAsync(Guid productId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<ReviewDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ReviewDto> CreateAsync(CreateReviewRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public class ReviewsApiService : BaseApiService, IReviewsApiService
{
    public ReviewsApiService(HttpClient httpClient) : base(httpClient) { }

    public Task<PagedResult<ReviewDto>> GetProductReviewsAsync(Guid productId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        => GetAsync<PagedResult<ReviewDto>>($"api/v1/reviews/product/{productId}?page={page}&pageSize={pageSize}", ct);

    public Task<ReviewDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        => GetAsync<ReviewDto>($"api/v1/reviews/{id}", ct);

    public Task<ReviewDto> CreateAsync(CreateReviewRequest request, CancellationToken ct = default)
        => PostAsync<CreateReviewRequest, ReviewDto>("api/v1/reviews", request, ct);

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => DeleteAsync($"api/v1/reviews/{id}", ct);
}
