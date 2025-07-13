using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Promotion;

public class NotifyPromotionUseCase : INotifyPromotionUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IValidator<NotifyPromotionRequest> _validator;

    public NotifyPromotionUseCase(
        IPromotionRepository promotionRepository,
        IValidator<NotifyPromotionRequest> validator)
    {
        _promotionRepository = promotionRepository;
        _validator = validator;
    }

    public async Task ExecuteAsync(NotifyPromotionRequest request, string tenantId)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var promotion = await _promotionRepository.GetByIdAsync(request.PromotionId, tenantId);

        // Placeholder: futura integração com API WhatsApp
        // Exemplo: await _whatsappService.SendMessageAsync(promotion, request.MessageTemplate);
    }
}