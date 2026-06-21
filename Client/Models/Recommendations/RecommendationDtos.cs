namespace Client.Models.Recommendations;

public record RecommendationDto(
    Guid ProductId,
    string Name,
    string? Description,
    decimal Price,
    decimal Rating,
    string? PrimaryImageUrl,
    string CategoryName,
    string VendorName,
    string ReasonLabel);
