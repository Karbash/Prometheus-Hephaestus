using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class UpdatePromotionRequestValidator : AbstractValidator<UpdatePromotionRequest>
{
    public UpdatePromotionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome é obrigatório.")
            .MaximumLength(100)
            .WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Descrição deve ter no máximo 500 caracteres.");

        RuleFor(x => x.DiscountType)
            .NotEmpty()
            .WithMessage("Tipo de desconto é obrigatório.")
            .IsInEnum()
            .WithMessage("Tipo de desconto inválido.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .WithMessage("Valor do desconto deve ser maior que zero.");

        RuleFor(x => x.MinOrderValue)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinOrderValue.HasValue)
            .WithMessage("Valor mínimo do pedido deve ser maior ou igual a zero.");

        RuleFor(x => x.MaxUsesPerCustomer)
            .GreaterThan(0)
            .When(x => x.MaxUsesPerCustomer.HasValue)
            .WithMessage("Uso máximo por cliente deve ser maior que zero.");

        RuleFor(x => x.MaxTotalUses)
            .GreaterThan(0)
            .When(x => x.MaxTotalUses.HasValue)
            .WithMessage("Uso total máximo deve ser maior que zero.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Data de início é obrigatória.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("Data de fim é obrigatória.")
            .GreaterThan(x => x.StartDate)
            .WithMessage("Data de fim deve ser posterior à data de início.");
    }
}
