using Client.Models.Search;

namespace Client.Services.Http;

public interface ISearchApiService
{
    Task<SearchResultDto> SearchAsync(SearchRequest request, string lang = "en", CancellationToken ct = default);
}

public class SearchApiService : BaseApiService, ISearchApiService
{
    public SearchApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<SearchResultDto> SearchAsync(SearchRequest request, string lang = "en", CancellationToken ct = default)
    {
        var queryParams = new List<string> { $"lang={lang}" };

        if (!string.IsNullOrEmpty(request.Keyword))
            queryParams.Add($"keyword={Uri.EscapeDataString(request.Keyword)}");

        if (request.CategoryId.HasValue)
            queryParams.Add($"categoryId={request.CategoryId.Value}");

        if (request.MinPrice.HasValue)
            queryParams.Add($"minPrice={request.MinPrice.Value}");

        if (request.MaxPrice.HasValue)
            queryParams.Add($"maxPrice={request.MaxPrice.Value}");

        if (request.MinRating.HasValue)
            queryParams.Add($"minRating={request.MinRating.Value}");

        if (!string.IsNullOrEmpty(request.SortBy))
            queryParams.Add($"sortBy={Uri.EscapeDataString(request.SortBy)}");

        queryParams.Add($"sortDescending={request.SortDescending}");
        queryParams.Add($"page={request.Page}");
        queryParams.Add($"pageSize={request.PageSize}");

        var queryString = string.Join("&", queryParams);
        return await GetAsync<SearchResultDto>($"api/v1/search?{queryString}", ct);
    }
}
