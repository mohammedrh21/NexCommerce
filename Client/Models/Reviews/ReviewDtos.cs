namespace Client.Models.Reviews;

public record CreateReviewRequest(
    Guid ProductId,
    int Rating,
    string Comment);

public record ReviewDto(
    Guid Id,
    Guid ProductId,
    Guid UserId,
    string ReviewerName,
    int Rating,
    string Comment,
    DateTime CreatedAt);
