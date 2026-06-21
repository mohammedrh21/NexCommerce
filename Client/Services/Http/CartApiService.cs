using Client.Models.Cart;

namespace Client.Services.Http;

public interface ICartApiService
{
    Task<CartDto> GetCartAsync(CancellationToken ct = default);
    Task<CartDto> AddItemAsync(AddToCartRequest request, CancellationToken ct = default);
    Task<CartDto> UpdateItemAsync(Guid itemId, UpdateCartItemRequest request, CancellationToken ct = default);
    Task<CartDto> RemoveItemAsync(Guid itemId, CancellationToken ct = default);
    Task ClearCartAsync(CancellationToken ct = default);
}

public class CartApiService : BaseApiService, ICartApiService
{
    public CartApiService(HttpClient httpClient) : base(httpClient) { }

    public Task<CartDto> GetCartAsync(CancellationToken ct = default)
        => GetAsync<CartDto>("api/v1/cart", ct);

    public Task<CartDto> AddItemAsync(AddToCartRequest request, CancellationToken ct = default)
        => PostAsync<AddToCartRequest, CartDto>("api/v1/cart/items", request, ct);

    public Task<CartDto> UpdateItemAsync(Guid itemId, UpdateCartItemRequest request, CancellationToken ct = default)
        => PutAsync<UpdateCartItemRequest, CartDto>($"api/v1/cart/items/{itemId}", request, ct);

    public Task<CartDto> RemoveItemAsync(Guid itemId, CancellationToken ct = default)
        => DeleteAsync<CartDto>($"api/v1/cart/items/{itemId}", ct);

    public Task ClearCartAsync(CancellationToken ct = default)
        => DeleteAsync("api/v1/cart", ct);
}
