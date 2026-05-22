using NexCommerce.Application.DTOs.Search;

namespace NexCommerce.Application.Contracts;

public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(SearchRequest request, Guid? requestingUserId, string lang, CancellationToken ct = default);
}
