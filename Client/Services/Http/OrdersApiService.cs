using System.Net.Http.Json;
using Client.Models.Common;
using Client.Models.Orders;

namespace Client.Services.Http;

public interface IOrdersApiService
{
    Task<PagedResult<OrderSummaryDto>> GetMyOrdersAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<PagedResult<OrderSummaryDto>> GetVendorOrdersAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<PagedResult<OrderSummaryDto>> GetAllOrdersAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<OrderDto> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken ct = default);
    Task<OrderSummaryDto> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken ct = default);
    Task CancelOrderAsync(Guid id, CancelOrderRequest request, CancellationToken ct = default);
}

public class OrdersApiService : BaseApiService, IOrdersApiService
{
    public OrdersApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<PagedResult<OrderSummaryDto>> GetMyOrdersAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return await GetAsync<PagedResult<OrderSummaryDto>>($"api/v1/orders/my-orders?page={page}&pageSize={pageSize}", ct);
    }

    public async Task<PagedResult<OrderSummaryDto>> GetVendorOrdersAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return await GetAsync<PagedResult<OrderSummaryDto>>($"api/v1/orders/vendor?page={page}&pageSize={pageSize}", ct);
    }

    public async Task<PagedResult<OrderSummaryDto>> GetAllOrdersAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return await GetAsync<PagedResult<OrderSummaryDto>>($"api/v1/orders?page={page}&pageSize={pageSize}", ct);
    }

    public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await GetAsync<OrderDto>($"api/v1/orders/{id}", ct);
    }

    public async Task<OrderDto> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken ct = default)
    {
        return await PostAsync<PlaceOrderRequest, OrderDto>("api/v1/orders", request, ct);
    }

    public async Task<OrderSummaryDto> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken ct = default)
    {
        return await PatchAsync<UpdateOrderStatusRequest, OrderSummaryDto>($"api/v1/orders/{id}/status", request, ct);
    }

    public async Task CancelOrderAsync(Guid id, CancelOrderRequest request, CancellationToken ct = default)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/orders/{id}/cancel")
        {
            Content = JsonContent.Create(request, typeof(CancelOrderRequest), options: JsonOptions)
        };
        var response = await HttpClient.SendAsync(httpRequest, ct);
        await HandleResponseAsync<object>(response, ct);
    }
}
