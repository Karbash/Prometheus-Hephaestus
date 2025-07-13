using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Promotion;

public class UpdatePromotionUseCase : IUpdatePromotionUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IValidator<UpdatePromotionRequest> _validator;
    private readonly IMenuItemRepository _menuItemRepository;

    public UpdatePromotionUseCase(
        IPromotionRepository promotionRepository,
        IValidator<UpdatePromotionRequest> validator,
        IMenuItemRepository menuItemRepository)
    {
        _promotionRepository = promotionRepository;
        _validator = validator;
        _menuItemRepository = menuItemRepository;
    }

    public async Task ExecuteAsync(string id, UpdatePromotionRequest request, string tenantId)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (request.Id != id)
            throw new ArgumentException("ID no corpo da requisição deve corresponder ao ID na URL.");

        var promotion = await _promotionRepository.GetByIdAsync(id, tenantId);

        if (request.DiscountType == "FreeItem" && !string.IsNullOrEmpty(request.MenuItemId))
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(request.MenuItemId, tenantId);
            if (menuItem == null)
                throw new InvalidOperationException("Item do cardápio não encontrado para FreeItem.");
        }

        if (!Enum.TryParse<DiscountType>(request.DiscountType, true, out var discountType))
            throw new InvalidOperationException("Tipo de desconto inválido.");

        promotion.Name = request.Name;
        promotion.Description = request.Description;
        promotion.DiscountType = discountType; // Convertido de string para enum
        promotion.DiscountValue = request.DiscountValue;
        promotion.MenuItemId = request.MenuItemId;
        promotion.MinOrderValue = request.MinOrderValue;
        promotion.MaxUsesPerCustomer = request.MaxUsagePerCustomer; // Ajustado para MaxUsesPerCustomer
        promotion.MaxTotalUses = request.MaxTotalUses;
        promotion.ApplicableTags = request.ApplicableToTags ?? new List<string>(); // Ajustado para ApplicableTags
        promotion.StartDate = request.StartDate;
        promotion.EndDate = request.EndDate;
        promotion.IsActive = request.IsActive;
        promotion.ImageUrl = request.ImageUrl;

        await _promotionRepository.UpdateAsync(promotion);
    }
}