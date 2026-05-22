using NexCommerce.Application.Common;
using NexCommerce.Application.Contracts;
using NexCommerce.Application.Contracts.Repositories;
using NexCommerce.Application.DTOs.Reviews;
using NexCommerce.Application.Exceptions;
using NexCommerce.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace NexCommerce.Application.Services;

public sealed class ReviewService(
    IReviewRepository reviewRepository,
    IProductRepository productRepository,
    IIdentityService identityService,
    IUnitOfWork unitOfWork,
    ILogger<ReviewService> logger) : IReviewService
{
    public async Task<PagedResult<ReviewDto>> GetProductReviewsAsync(Guid productId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await reviewRepository.GetByProductPagedAsync(productId, page, pageSize, ct);

        var reviewDtos = new List<ReviewDto>();
        foreach (var r in items)
        {
            var reviewerName = "Customer";
            try
            {
                var user = await identityService.GetUserInfoAsync(r.UserId);
                reviewerName = user?.FullName ?? "Customer";
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to resolve reviewer name for user {UserId}", r.UserId);
            }
            reviewDtos.Add(new ReviewDto(r.Id, r.ProductId, r.UserId, reviewerName, r.Rating, r.Comment, r.CreatedAt));
        }

        return new PagedResult<ReviewDto>
        {
            Data = reviewDtos,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ReviewDto> GetByIdAsync(Guid reviewId, CancellationToken ct = default)
    {
        var r = await reviewRepository.FindByIdAsync(reviewId, ct)
            ?? throw new NotFoundException($"Review {reviewId} not found.");

        var reviewerName = "Customer";
        try
        {
            var user = await identityService.GetUserInfoAsync(r.UserId);
            reviewerName = user?.FullName ?? "Customer";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resolve reviewer name for user {UserId}", r.UserId);
        }

        return new ReviewDto(r.Id, r.ProductId, r.UserId, reviewerName, r.Rating, r.Comment, r.CreatedAt);
    }

    public async Task<ReviewDto> CreateAsync(Guid userId, CreateReviewRequest request, CancellationToken ct = default)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new ValidationException(new List<string> { "Rating must be between 1 and 5." });
        }

        var product = await productRepository.FindWithDetailsAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Product {request.ProductId} not found.");

        var alreadyReviewed = await reviewRepository.HasUserReviewedAsync(userId, request.ProductId, ct);
        if (alreadyReviewed)
        {
            throw new ValidationException(new List<string> { "You have already reviewed this product." });
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = request.ProductId,
            Rating = request.Rating,
            Comment = request.Comment ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        reviewRepository.Add(review);
        await unitOfWork.SaveChangesAsync(ct);

        // Denormalize product rating
        var avg = await reviewRepository.GetAverageRatingAsync(request.ProductId, ct);
        product.Rating = avg;
        await unitOfWork.SaveChangesAsync(ct);

        var reviewerName = "Customer";
        try
        {
            var user = await identityService.GetUserInfoAsync(userId);
            reviewerName = user?.FullName ?? "Customer";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resolve reviewer name for user {UserId}", userId);
        }

        return new ReviewDto(review.Id, review.ProductId, review.UserId, reviewerName, review.Rating, review.Comment, review.CreatedAt);
    }

    public async Task DeleteAsync(Guid reviewId, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var review = await reviewRepository.FindByIdAsync(reviewId, ct)
            ?? throw new NotFoundException($"Review {reviewId} not found.");

        var isAdmin = roles.Contains("Admin");
        var isAuthor = review.UserId == requestingUserId;
        var isProductOwner = await productRepository.IsVendorOwnerAsync(review.ProductId, requestingUserId, ct);

        if (!isAdmin && !isAuthor && !isProductOwner)
        {
            throw new ForbiddenException("You are not authorized to delete this review.");
        }

        var productId = review.ProductId;
        reviewRepository.Remove(review);
        await unitOfWork.SaveChangesAsync(ct);

        // Recalculate and update product rating after deletion
        var product = await productRepository.FindWithDetailsAsync(productId, ct);
        if (product is not null)
        {
            var avg = await reviewRepository.GetAverageRatingAsync(productId, ct);
            product.Rating = avg;
            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
