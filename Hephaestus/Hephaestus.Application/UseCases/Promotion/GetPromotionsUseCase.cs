using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Promotion;

public class GetPromotionsUseCase : IGetPromotionsUseCase
{
    private readonly IPromotionRepository _promotionRepository;

    public GetPromotionsUseCase(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<IEnumerable<PromotionResponse>> ExecuteAsync(string tenantId, bool? isActive)
    {
        var promotions = await _promotionRepository.GetByTenantIdAsync(tenantId, isActive);
        return promotions.Select(p => new PromotionResponse
        {
            Id = p.Id,
            TenantId = p.TenantId,
            Name = p.Name,
            Description = p.Description,
            DiscountType = p.DiscountType.ToString(), // Convertendo enum para string
            DiscountValue = p.DiscountValue,
            MenuItemId = p.MenuItemId,
            MinOrderValue = p.MinOrderValue,
            MaxUsagePerCustomer = p.MaxUsesPerCustomer, // Corrigido de MaxUsagePerCustomer para MaxUsesPerCustomer
            MaxTotalUses = p.MaxTotalUses,
            ApplicableToTags = p.ApplicableTags, // Corrigido de ApplicableToTags para ApplicableTags
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = p.IsActive,
            ImageUrl = p.ImageUrl
        }).ToList();
    }
}