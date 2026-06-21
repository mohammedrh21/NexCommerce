using Client.Models.Common;
using Client.Models.Products;

namespace Client.Services.Http;

public interface IWishlistsApiService
{
    Task<PagedResult<ProductListItemDto>> GetWishlistAsync(int page = 1, int pageSize = 20, string lang = "en", CancellationToken ct = default);
    Task AddToWishlistAsync(Guid productId, CancellationToken ct = default);
    Task RemoveFromWishlistAsync(Guid productId, CancellationToken ct = default);
    Task<bool> CheckWishlistAsync(Guid productId, CancellationToken ct = default);
}

public class WishlistsApiService : BaseApiService, IWishlistsApiService
{
    public WishlistsApiService(HttpClient httpClient) : base(httpClient) { }

    public Task<PagedResult<ProductListItemDto>> GetWishlistAsync(int page = 1, int pageSize = 20, string lang = "en", CancellationToken ct = default)
        => GetAsync<PagedResult<ProductListItemDto>>($"api/v1/wishlists?page={page}&pageSize={pageSize}&lang={lang}", ct);

    public Task AddToWishlistAsync(Guid productId, CancellationToken ct = default)
        => PostAsync<object, object>($"api/v1/wishlists/{productId}", new { }, ct);

    public Task RemoveFromWishlistAsync(Guid productId, CancellationToken ct = default)
        => DeleteAsync($"api/v1/wishlists/{productId}", ct);

    public Task<bool> CheckWishlistAsync(Guid productId, CancellationToken ct = default)
        => GetAsync<bool>($"api/v1/wishlists/{productId}/check", ct);
}
