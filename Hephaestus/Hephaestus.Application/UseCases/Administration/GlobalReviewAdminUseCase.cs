using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Application.Services;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Application.UseCases.Administration;

public class GlobalReviewAdminUseCase : BaseUseCase, IGlobalReviewAdminUseCase
{
    private readonly IReviewRepository _reviewRepository;

    public GlobalReviewAdminUseCase(
        IReviewRepository reviewRepository,
        ILogger<GlobalReviewAdminUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<PagedResult<ReviewResponse>> ExecuteAsync(
        string? companyId = null,
        string? customerId = null,
        string? orderId = null,
        int? ratingMin = null,
        int? ratingMax = null,
        bool? isActive = null,
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var pagedReviews = await _reviewRepository.GetAllGlobalAsync(
                companyId,
                customerId,
                orderId,
                ratingMin,
                ratingMax,
                isActive,
                dataInicial,
                dataFinal,
                pageNumber,
                pageSize,
                sortBy,
                sortOrder);

            var reviewResponses = pagedReviews.Items.Select(r => new ReviewResponse
            {
                Id = r.Id,
                TenantId = r.TenantId,
                CustomerId = r.CustomerId,
                OrderId = r.OrderId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                IsActive = false, // Valor padrão, pois não existe na entidade
                CreatedBy = null, // Valor padrão
                UpdatedBy = null  // Valor padrão
            }).ToList();

            return new PagedResult<ReviewResponse>
            {
                Items = reviewResponses,
                TotalCount = pagedReviews.TotalCount,
                PageNumber = pagedReviews.PageNumber,
                PageSize = pagedReviews.PageSize
            };
        }, "GlobalReviewAdmin");
    }
} 