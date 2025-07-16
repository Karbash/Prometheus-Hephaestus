using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class NotifyPromotionRequestValidator : AbstractValidator<NotifyPromotionRequest>
{
    public NotifyPromotionRequestValidator()
    {
        RuleFor(x => x.PromotionId)
            .NotEmpty().WithMessage("ID da promo��o � obrigat�rio.")
            .MaximumLength(36).WithMessage("ID da promo��o deve ter no m�ximo 36 caracteres.");

        RuleFor(x => x.MessageTemplate)
            .NotEmpty().WithMessage("Modelo de mensagem � obrigat�rio.")
            .MaximumLength(1000).WithMessage("Modelo de mensagem n�o pode exceder 1000 caracteres.");
    }
}
