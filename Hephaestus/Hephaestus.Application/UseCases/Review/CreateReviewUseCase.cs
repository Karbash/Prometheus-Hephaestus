using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Review;

public class CreateReviewUseCase
{
    private readonly IReviewRepository _reviewRepository;

    public CreateReviewUseCase(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<ReviewResponse> ExecuteAsync(CreateReviewRequest request)
    {
        var review = new Hephaestus.Domain.Entities.Review
        {
            TenantId = request.TenantId,
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        review.SetStarsFromRating();
        await _reviewRepository.AddAsync(review);
        return new ReviewResponse
        {
            Id = review.Id,
            TenantId = review.TenantId,
            OrderId = review.OrderId,
            CustomerId = review.CustomerId,
            Rating = review.Rating,
            Stars = review.Stars,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }
} 