using FluentValidation;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.Enum;

namespace Hephaestus.Application.Validators;

public class CreatePromotionRequestValidator : AbstractValidator<CreatePromotionRequest>
{
    public CreatePromotionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome não pode exceder 100 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(500).WithMessage("Descrição não pode exceder 500 caracteres.");

        RuleFor(x => x.DiscountType)
            .IsInEnum()
            .WithMessage("Tipo de desconto inválido. Use: Percentage, Fixed ou FreeItem.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Valor do desconto deve ser maior que zero.");

        RuleFor(x => x.MenuItemId)
            .NotEmpty()
            .When(x => x.DiscountType == DiscountType.FreeItem)
            .WithMessage("MenuItemId é obrigatório para DiscountType FreeItem.");

        RuleFor(x => x.MinOrderValue)
            .GreaterThanOrEqualTo(0).When(x => x.MinOrderValue.HasValue)
            .WithMessage("Valor mínimo do pedido deve ser maior ou igual a zero.");

        RuleFor(x => x.MaxUsesPerCustomer)
            .GreaterThan(0).When(x => x.MaxUsesPerCustomer.HasValue)
            .WithMessage("Máximo de usos por cliente deve ser maior que zero.");

        RuleFor(x => x.MaxTotalUses)
            .GreaterThan(0).When(x => x.MaxTotalUses.HasValue)
            .WithMessage("Máximo de usos totais deve ser maior que zero.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Data de início é obrigatória.")
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Data de início deve ser anterior ou igual à data de término.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Data de término é obrigatória.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Data de término deve ser posterior ou igual à data de início.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("URL da imagem não pode exceder 500 caracteres.");
    }
}
