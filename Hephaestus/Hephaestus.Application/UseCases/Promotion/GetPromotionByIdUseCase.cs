using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Promotion;

public class GetPromotionByIdUseCase : IGetPromotionByIdUseCase
{
    private readonly IPromotionRepository _promotionRepository;

    public GetPromotionByIdUseCase(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<PromotionResponse> ExecuteAsync(string id, string tenantId)
    {
        var promotion = await _promotionRepository.GetByIdAsync(id, tenantId);
        if (promotion == null)
            throw new KeyNotFoundException("Promoção não encontrada.");

        return new PromotionResponse
        {
            Id = promotion.Id,
            TenantId = promotion.TenantId,
            Name = promotion.Name,
            Description = promotion.Description,
            DiscountType = promotion.DiscountType.ToString(), // Convertendo enum para string
            DiscountValue = promotion.DiscountValue,
            MenuItemId = promotion.MenuItemId,
            MinOrderValue = promotion.MinOrderValue,
            MaxUsagePerCustomer = promotion.MaxUsesPerCustomer, // Corrigido para MaxUsesPerCustomer
            MaxTotalUses = promotion.MaxTotalUses,
            ApplicableToTags = promotion.ApplicableTags, // Corrigido para ApplicableTags
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            IsActive = promotion.IsActive,
            ImageUrl = promotion.ImageUrl
        };
    }
}