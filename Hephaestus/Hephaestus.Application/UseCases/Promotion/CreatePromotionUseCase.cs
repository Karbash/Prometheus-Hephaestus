using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Promotion;

public class CreatePromotionUseCase : ICreatePromotionUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IValidator<CreatePromotionRequest> _validator;
    private readonly IMenuItemRepository _menuItemRepository;

    public CreatePromotionUseCase(
        IPromotionRepository promotionRepository,
        IValidator<CreatePromotionRequest> validator,
        IMenuItemRepository menuItemRepository)
    {
        _promotionRepository = promotionRepository;
        _validator = validator;
        _menuItemRepository = menuItemRepository;
    }

    public async Task<string> ExecuteAsync(CreatePromotionRequest request, string tenantId)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (request.DiscountType == "FreeItem" && !string.IsNullOrEmpty(request.MenuItemId))
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(request.MenuItemId, tenantId);
            if (menuItem == null)
                throw new InvalidOperationException("Item do cardápio não encontrado para FreeItem.");
        }

        if (!Enum.TryParse<DiscountType>(request.DiscountType, true, out var discountType))
            throw new InvalidOperationException("Tipo de desconto inválido.");

        var promotion = new Domain.Entities.Promotion
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            DiscountType = discountType, // Convertido de string para enum
            DiscountValue = request.DiscountValue,
            MenuItemId = request.MenuItemId,
            MinOrderValue = request.MinOrderValue,
            MaxUsesPerCustomer = request.MaxUsagePerCustomer, // Ajustado para MaxUsesPerCustomer
            MaxTotalUses = request.MaxTotalUses,
            ApplicableTags = request.ApplicableToTags ?? new List<string>(), // Ajustado para ApplicableTags
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            ImageUrl = request.ImageUrl
        };

        await _promotionRepository.AddAsync(promotion);
        return promotion.Id;
    }
}