namespace NexCommerce.Application.DTOs.Reviews;

// ── Requests ─────────────────────────────────────────────────────────────────

public record CreateReviewRequest(
    Guid ProductId,
    int Rating,
    string Comment);

// ── Response DTOs ─────────────────────────────────────────────────────────────

public record ReviewDto(
    Guid Id,
    Guid ProductId,
    Guid UserId,
    string ReviewerName,
    int Rating,
    string Comment,
    DateTime CreatedAt);
