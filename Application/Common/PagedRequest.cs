namespace NexCommerce.Application.Common;

public class PagedRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? SortBy { get; set; }
    public string SortDir { get; set; } = "asc";
    public string? Keyword { get; set; }
}
