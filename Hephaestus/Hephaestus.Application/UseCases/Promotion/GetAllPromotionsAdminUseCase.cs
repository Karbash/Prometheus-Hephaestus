using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using System.Linq;

namespace Hephaestus.Application.UseCases.Promotion;

public class GetAllPromotionsAdminUseCase : BaseUseCase, IGetAllPromotionsAdminUseCase
{
    private readonly IPromotionRepository _promotionRepository;

    public GetAllPromotionsAdminUseCase(
        IPromotionRepository promotionRepository,
        ILogger<GetAllPromotionsAdminUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<PagedResult<PromotionResponse>> ExecuteAsync(bool? isActive = null, string? companyId = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var pagedPromotions = await _promotionRepository.GetAllGlobalAsync(null, isActive, companyId, pageNumber, pageSize, sortBy, sortOrder);
            return new PagedResult<PromotionResponse>
            {
                Items = pagedPromotions.Items.Select(p => new PromotionResponse
                {
                    Id = p.Id,
                    CompanyId = p.CompanyId,
                    Name = p.Name,
                    Description = p.Description,
                    DiscountType = p.DiscountType,
                    DiscountValue = p.DiscountValue,
                    MenuItemId = p.MenuItemId,
                    MinOrderValue = p.MinOrderValue,
                    MaxUsesPerCustomer = p.MaxUsesPerCustomer,
                    MaxTotalUses = p.MaxTotalUses,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsActive = p.IsActive,
                    ImageUrl = p.ImageUrl
                }).ToList(),
                TotalCount = pagedPromotions.TotalCount,
                PageNumber = pagedPromotions.PageNumber,
                PageSize = pagedPromotions.PageSize
            };
        });
    }
} 