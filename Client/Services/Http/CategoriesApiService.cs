using Client.Models.Categories;

namespace Client.Services.Http;

public interface ICategoriesApiService
{
    Task<IEnumerable<CategoryListItemDto>> GetAllAsync(string lang = "en", CancellationToken ct = default);
    Task<IEnumerable<CategoryListItemDto>> GetRootsAsync(string lang = "en", CancellationToken ct = default);
    Task<IEnumerable<CategoryListItemDto>> GetSubcategoriesAsync(Guid id, string lang = "en", CancellationToken ct = default);
    Task<CategoryDto> GetByIdAsync(Guid id, string lang = "en", CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public class CategoriesApiService : BaseApiService, ICategoriesApiService
{
    public CategoriesApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IEnumerable<CategoryListItemDto>> GetAllAsync(string lang = "en", CancellationToken ct = default)
    {
        return await GetAsync<IEnumerable<CategoryListItemDto>>($"api/v1/categories?lang={lang}", ct);
    }

    public async Task<IEnumerable<CategoryListItemDto>> GetRootsAsync(string lang = "en", CancellationToken ct = default)
    {
        return await GetAsync<IEnumerable<CategoryListItemDto>>($"api/v1/categories/root?lang={lang}", ct);
    }

    public async Task<IEnumerable<CategoryListItemDto>> GetSubcategoriesAsync(Guid id, string lang = "en", CancellationToken ct = default)
    {
        return await GetAsync<IEnumerable<CategoryListItemDto>>($"api/v1/categories/{id}/subcategories?lang={lang}", ct);
    }

    public async Task<CategoryDto> GetByIdAsync(Guid id, string lang = "en", CancellationToken ct = default)
    {
        return await GetAsync<CategoryDto>($"api/v1/categories/{id}?lang={lang}", ct);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        return await PostAsync<CreateCategoryRequest, CategoryDto>("api/v1/categories", request, ct);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        return await PutAsync<UpdateCategoryRequest, CategoryDto>($"api/v1/categories/{id}", request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await DeleteAsync($"api/v1/categories/{id}", ct);
    }
}
