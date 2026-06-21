using Client.Models.Common;
using Client.Models.Products;

namespace Client.Services.Http;

public interface IProductsApiService
{
    Task<PagedResult<ProductListItemDto>> GetAllAsync(int page = 1, int pageSize = 10, string lang = "en", CancellationToken ct = default);
    Task<ProductDetailDto> GetByIdAsync(Guid id, string lang = "en", CancellationToken ct = default);
    Task<PagedResult<ProductListItemDto>> GetByVendorAsync(Guid vendorId, int page = 1, int pageSize = 10, string lang = "en", CancellationToken ct = default);
    Task<PagedResult<ProductListItemDto>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 10, string lang = "en", CancellationToken ct = default);
    Task<ProductDetailDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDetailDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public class ProductsApiService : BaseApiService, IProductsApiService
{
    public ProductsApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<PagedResult<ProductListItemDto>> GetAllAsync(int page = 1, int pageSize = 10, string lang = "en", CancellationToken ct = default)
    {
        return await GetAsync<PagedResult<ProductListItemDto>>($"api/v1/products?page={page}&pageSize={pageSize}&lang={lang}", ct);
    }

    public async Task<ProductDetailDto> GetByIdAsync(Guid id, string lang = "en", CancellationToken ct = default)
    {
        return await GetAsync<ProductDetailDto>($"api/v1/products/{id}?lang={lang}", ct);
    }

    public async Task<PagedResult<ProductListItemDto>> GetByVendorAsync(Guid vendorId, int page = 1, int pageSize = 10, string lang = "en", CancellationToken ct = default)
    {
        return await GetAsync<PagedResult<ProductListItemDto>>($"api/v1/products/vendor/{vendorId}?page={page}&pageSize={pageSize}&lang={lang}", ct);
    }

    public async Task<PagedResult<ProductListItemDto>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 10, string lang = "en", CancellationToken ct = default)
    {
        return await GetAsync<PagedResult<ProductListItemDto>>($"api/v1/products/category/{categoryId}?page={page}&pageSize={pageSize}&lang={lang}", ct);
    }

    public async Task<ProductDetailDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        return await PostAsync<CreateProductRequest, ProductDetailDto>("api/v1/products", request, ct);
    }

    public async Task<ProductDetailDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        return await PutAsync<UpdateProductRequest, ProductDetailDto>($"api/v1/products/{id}", request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await DeleteAsync($"api/v1/products/{id}", ct);
    }
}
