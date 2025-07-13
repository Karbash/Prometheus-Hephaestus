using FluentValidation;
using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class NotifyPromotionRequestValidator : AbstractValidator<NotifyPromotionRequest>
{
    public NotifyPromotionRequestValidator()
    {
        RuleFor(x => x.PromotionId)
            .NotEmpty().WithMessage("ID da promoção é obrigatório.")
            .MaximumLength(36).WithMessage("ID da promoção deve ter no máximo 36 caracteres.");

        RuleFor(x => x.MessageTemplate)
            .NotEmpty().WithMessage("Modelo de mensagem é obrigatório.")
            .MaximumLength(1000).WithMessage("Modelo de mensagem não pode exceder 1000 caracteres.");
    }
}